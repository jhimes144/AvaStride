using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Stride.Core;
using Stride.Engine;

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

        /// <summary>
        /// Initializes and attaches the Avalonia UI framework to the specified game. This method should be called before using any Avalonia related functionality in the game.
        /// </summary>
        /// <param name="game">The game instance to which Avalonia UI will be attached.</param>
        /// <param name="appBuilder">The Avalonia application builder to configure and start the Avalonia application.</param>
        /// <param name="waitForWindowInit">Indicates whether to wait for the Avalonia window to be initialized before continuing execution.</param>
        /// <exception cref="InvalidOperationException">Thrown if Avalonia has already been initialized with a game.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown if the current operating system is not supported.</exception>
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
            _game.GameSystems.Add(_gameCallbacks = new GameCallbackSystem(game.Services, true));
        }

        /// <summary>
        /// Initializes Avalonia with the specified window. This method sets up the necessary configurations and displays the window in full-screen mode.
        /// </summary>
        /// <param name="window">The Avalonia window to initialize and display.</param>
        /// <param name="enableCaptureAtStart">Determines whether to enable input capture for the window at startup.</param>
        /// <param name="applyTransparency">Indicates whether to apply transparency settings to the window.</param>
        /// <exception cref="InvalidOperationException">Thrown if a window has already been initialized.</exception>
        /// <exception cref="Exception">Thrown if there is an error in finding the top-level window or its handle.</exception>
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
        /// Note: Any cursor style will also change to whatever is set in the UI.
        /// </summary>
        /// <param name="enable"></param>
        public static void EnableUICapture(bool enable)
        {
            checkThrowWindowInit();

            NativeHelper.EnableWindow(_avaHandle, enable);
            NativeHelper.SetForegroundWindow(enable ? _avaHandle : _gameHandle);
            NativeHelper.SetActiveWindow(enable ? _avaHandle : _gameHandle);
            NativeHelper.SetFocus(enable ? _avaHandle : _gameHandle);
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
        /// Sets the priority of game callbacks. This method is useful for managing the order in which updates are processed within the game.
        /// </summary>
        /// <param name="priority">The priority level to set for the game callbacks.</param>
        public static void SetGameCallbacksPriority(int priority)
        {
            DispatchGameThread(_ => _gameCallbacks!.UpdateOrder = priority);
        }

        /// <summary>
        /// Dispatches an action on the game thread, to be called at next frame update.
        /// If the executing code is already on the game thread, than the action is ran immediately.
        /// </summary>
        /// <param name="action"></param>
        public static void DispatchGameThread(Action<Game> action)
        {
            checkThrowGameInit();

            // this does a simple reference check on current thread.
            if (_gameCallbacks!.CheckAccess())
            {
                action(_game!);
            }

            _gameCallbacks!.Dispatch(() => action(_game!));
        }

        /// <summary>
        /// Dispatches an action on the UI thread.
        /// If the executing code is already on the UI thread, than the action is ran immediately.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        public static void DispatchUIThread(Action<Window> action, DispatcherPriority priority = default)
        {
            checkThrowWindowInit();

            // this does a simple reference check on current thread.
            if (Dispatcher.UIThread.CheckAccess()) 
            {
                action(_uiWindow!);
                return;
            }

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
