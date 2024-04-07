using AvaStride;
using FirstPersonShooter.UI;
using Stride.Core;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstPersonShooter
{
    /// <summary>
    /// Used to show and hide main menu.
    /// </summary>
    internal class MainMenuService
    {
        readonly Game _game;

        public MainMenuService(Game game)
        {
            _game = game;

            AvaloniaInStride.DispatchUIThread(window =>
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.ViewModel.ContinueGameClick += (_, __) =>
                    {
                        AvaloniaInStride.EnableUICapture(false);
                        mainWindow.ViewModel.MainMenuVisible = false;

                        AvaloniaInStride.DispatchGameThread(game =>
                        {
                            game.Input.LockMousePosition();
                        });
                    };

                    mainWindow.ViewModel.ExitGameClick += (_, __) =>
                    {
                        AvaloniaInStride.DispatchGameThread(game =>
                        {
                            game.Exit();
                        });
                    };
                }
            });
        }

        public void ShowMainMenu()
        {
            _game.Input.UnlockMousePosition();
            AvaloniaInStride.EnableUICapture(true);
            AvaloniaInStride.DispatchUIThread(window =>
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.ViewModel.MainMenuVisible = true;
                }
            });
        }
    }
}
