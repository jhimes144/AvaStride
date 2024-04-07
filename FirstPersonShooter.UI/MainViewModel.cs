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
        public event EventHandler? ContinueGameClick;
        public event EventHandler? ExitGameClick;

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
            ContinueGameCommand = ReactiveCommand.Create(() => ContinueGameClick?.Invoke(this, EventArgs.Empty));
            ExitGameCommand = ReactiveCommand.Create(() => ExitGameClick?.Invoke(this, EventArgs.Empty));
        }
    }
}
