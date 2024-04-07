using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Stride.Core;
using Stride.Engine;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vortice.Vulkan;

namespace AvaStride
{
    public static class AvaloniaInStride
    {
        readonly static HashSet<Action<Window, TimeSpan>> _renderCallbacks = [];
        readonly static object _uiRenderCallbacksLock = new();

        static bool _uiRenderCallbacksOn;
        static Game? _game;
        static nint _gameHandle;
        static nint _avaHandle;
        static Window? _uiWindow;
        static AutoResetEvent? _windowAvailEvent;
		static GameCallbackSystem? _gameCallbacks;

		public static bool GameAttached => _game != null;

        public static bool UIAttached => _uiWindow != null;

        public static void StartAndAttachAvalonia(Game game, AppBuilder appBuilder, bool waitForWindowInit = true)
        {
            if (_gameHandle != IntPtr.Zero)
            {
                throw new InvalidOperationException("Avalonia already initialized with game.");
            }

            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("AvaStride is currently only compatiable with Windows.");
            }

            var avaloniaThread = new Thread(avaloniaThreadM)
            {
                IsBackground = true,
                Name = "Avalonia UI"
            };

            avaloniaThread.SetApartmentState(ApartmentState.STA);
            avaloniaThread.Start(appBuilder);

            _game = game;
            _gameHandle = game.Window.NativeWindow.Handle;

            if (waitForWindowInit)
            {
                _windowAvailEvent = new AutoResetEvent(false);
                _windowAvailEvent.WaitOne();
                _windowAvailEvent.Dispose();
            }

            // Add the game callback system to the game.
            _game.GameSystems.Add(_gameCallbacks = new GameCallbackSystem(game.Services));
        }

        public static void InitializeWithWindow(Window window, bool enableCaptureAtStart, bool applyTransparency = true)
        {
            if (_uiWindow != null)
            {
                throw new InvalidOperationException("Window has already been initialized.");
            }

            checkThrowGameInit();

            if (TopLevel.GetTopLevel(window) is not { } topLevel)
            {
                throw new Exception("Failed to find TopLevel for Avalonia app.");
            }

            if (topLevel.TryGetPlatformHandle() is not { } pHandle)
            {
                throw new Exception("Failed to get window handle for Avalonia app.");
            }

            _avaHandle = pHandle.Handle;

            if (applyTransparency)
            {
                window.TransparencyLevelHint = new List<WindowTransparencyLevel> { WindowTransparencyLevel.Transparent };
                window.Background = Brushes.Transparent;
            }

            window.Show();
            window.WindowState = WindowState.FullScreen;

            NativeHelper.SetWindowLong(_avaHandle, NativeHelper.GWL_STYLE, NativeHelper.WS_CHILD | NativeHelper.WS_VISIBLE);
            NativeHelper.SetWindowLong(_avaHandle, NativeHelper.GWL_EXSTYLE, (int)NativeHelper.WS_EX_LAYERED);
            NativeHelper.SetParent(_avaHandle, _gameHandle);

            _uiWindow = window;
            _windowAvailEvent?.Set();

            if (!enableCaptureAtStart)
            {
                NativeHelper.EnableWindow(_avaHandle, false);
            }
        }

        /// <summary>
        /// Sets whether the UI is to receive mouse and keyboard events. When the UI is enabled, the underlying game will NOT receive keyboard and mouse events.
        /// However, any explicit mouse input captures may still run. Its a good idea to stop any explicit captures in-game when enabling the UI.
        /// </summary>
        /// <param name="enable"></param>
        public static void EnableUICapture(bool enable)
        {
            checkThrowWindowInit();
            NativeHelper.EnableWindow(_avaHandle, enable);
        }

        /// <summary>
        /// Sets the visibility of the UI. When the UI is not visible, it is also disabled.
        /// </summary>
        /// <param name="visible"></param>
        public static void SetUIIsVisible(bool visible)
        {
            var style = NativeHelper.GetWindowLong(_avaHandle, NativeHelper.GWL_STYLE);

            if (visible)
            {
                style |= NativeHelper.WS_VISIBLE;
            }
            else
            {
                style &= ~NativeHelper.WS_VISIBLE;
            }

            NativeHelper.SetWindowLong(_avaHandle, NativeHelper.GWL_STYLE, style);
        }

        /// <summary>
        /// Sets the priority of game callbacks as a script priority level.
        /// </summary>
        /// <param name="priority"></param>
        public static void SetGameCallbacksPriority(int priority)
        {
            DispatchGameThread(_ => _gameCallbacks.UpdateOrder = priority);
        }

        /// <summary>
        /// Dispatches an action on the game thread, to be called at next frame update.
        /// </summary>
        /// <param name="action"></param>
        public static void DispatchGameThread(Action<Game> action)
        {
            checkThrowGameInit();
            _gameCallbacks.Dispatch(() => action(_game!));
        }

        /// <summary>
        /// Dispatches an action on the UI thread.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        public static void DispatchUIThread(Action<Window> action, DispatcherPriority priority = default)
        {
            checkThrowWindowInit();
            Dispatcher.UIThread.Post(() => action(_uiWindow!), priority);
        }

        /// <summary>
        /// Registers a UISyncScript. This is called automatically when a UISyncScript is started.
        /// </summary>
        /// <param name="script"></param>
        public static void RegisterUIThreadScript(UISyncScript script)
        {
            var trash = RegisterUIThreadUpdateCallback(script.UIUpdate);
            script.Collector.Add(trash);
        }

        /// <summary>
        /// Registers a callback to run on the UI thread for every UI update. Returns a disposable handle to remove callback. See remarks.
        /// </summary>
        /// <remarks>
        /// Avalonia's UI thread updates are often throttled to only what is needed. For example, a static UI will not cause the UI thread 
        /// to update until the user interacts with the UI or an animation is ran. When using this method to register a callback, it places to UI thread in a constant 60fps update
        /// until the callback is disposed. This has a small cpu penalty. This method is much more performant vs dispatches when you are rapidly updating the UI.
        /// </remarks>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IDisposable RegisterUIThreadUpdateCallback(Action<Window, TimeSpan> action)
        {
            checkThrowWindowInit();

            lock (_uiRenderCallbacksLock)
            {
                _renderCallbacks.Add(action);

                if (!_uiRenderCallbacksOn)
                {
                    _uiRenderCallbacksOn = true;

                    DispatchUIThread(window =>
                    {
                        var top = TopLevel.GetTopLevel(window)!;

                        void frame(TimeSpan t)
                        {
                            lock (_uiRenderCallbacksLock)
                            {
                                if (!_uiRenderCallbacksOn)
                                {
                                    return;
                                }

                                foreach (var callback in _renderCallbacks)
                                {
                                    callback(window, t);
                                }
                            }

                            top.RequestAnimationFrame(frame);
                        }

                        top.RequestAnimationFrame(frame);
                    });
                }
            }

            return new AnonymousDisposable(() =>
            {
                lock (_uiRenderCallbacksLock)
                {
                    _renderCallbacks.Remove(action);

                    if (_renderCallbacks.Count == 0)
                    {
                        _uiRenderCallbacksOn = false;
                    }
                }
            });
        }

        static void checkThrowWindowInit()
        {
            if (_uiWindow == null)
            {
                throw new InvalidOperationException("Avalonia window not initialized.");
            }
        }

        static void checkThrowGameInit()
        {
            if (_game == null)
            {
                throw new InvalidOperationException("Game not initialized.");
            }
        }

        static void avaloniaThreadM(object? state)
        {
            if (state is AppBuilder appBuilder)
            {
                appBuilder.StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs());
            }
            else
            {
                throw new InvalidOperationException("Was expecting appbuilder instance.");
            }
        }
    }
}
