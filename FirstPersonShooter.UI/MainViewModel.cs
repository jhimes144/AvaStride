using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstPersonShooter.UI
{
    public class MainViewModel : ReactiveObject
    {
        [Reactive]
        public string? ShotsFired { get; set; } = "0";

        [Reactive]
        public string? Location { get; set; } = "--";
    }
}
