using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaStride;
using ReactiveUI;

namespace FirstPersonShooter.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                if (AvaloniaInStride.GameAttached)
                {
                    AvaloniaInStride.InitializeWithWindow(new MainWindow(), false, true);
                }
                else
                {
                    desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
                }
            }

            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

            base.OnFrameworkInitializationCompleted();
        }
    }
}