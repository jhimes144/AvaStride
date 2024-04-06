using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FirstPersonShooter.UI
{
    public class Program
    {
        // NOTE: Program.Main will not be ran when this app is attached to Stride, but its here for two reasons:
        // 1. The avalonia xaml autocomplete and designer are able to function
        // 2. The ui can be tested independently from the game (if desired)

        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
