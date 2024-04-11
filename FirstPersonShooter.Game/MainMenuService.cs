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
