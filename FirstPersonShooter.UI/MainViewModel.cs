using AvaStride;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FirstPersonShooter.UI
{
    public class MainViewModel : ReactiveObject
    {
        [Reactive]
        public string? ShotsFired { get; set; } = "0";

        [Reactive]
        public string? Location { get; set; } = "--";

        [Reactive]
        public bool MainMenuVisible { get; set; }

        public ICommand ContinueGameCommand { get; }

        public ICommand ExitGameCommand { get; }

        public MainViewModel()
        {
            ContinueGameCommand = ReactiveCommand.Create(continueGame);
            ExitGameCommand = ReactiveCommand.Create(exitGame);
        }

        void continueGame()
        {
            AvaloniaInStride.EnableUICapture(false);
            MainMenuVisible = false;

            AvaloniaInStride.DispatchGameThread(game => game.Input.LockMousePosition());
        }

        void exitGame()
        {
            AvaloniaInStride.DispatchGameThread(game => game.Exit());
        }
    }
}
