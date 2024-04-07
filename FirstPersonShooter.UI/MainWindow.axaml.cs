using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace FirstPersonShooter.UI
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            var shotsFiredScaleTransform = new ScaleTransform(1, 1);
            ShotsFiredTxt.RenderTransform = shotsFiredScaleTransform;

            var growAni = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new SineEaseIn(),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 1d),
                            new Setter(ScaleTransform.ScaleYProperty, 1d)
                        },
                        KeyTime = TimeSpan.Zero
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 1.5d),
                            new Setter(ScaleTransform.ScaleYProperty, 1.5d)
                        },
                        KeyTime = TimeSpan.FromMilliseconds(100)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 1d),
                            new Setter(ScaleTransform.ScaleYProperty, 1d)
                        },
                        KeyTime = TimeSpan.FromMilliseconds(200)
                    }
                }
            };

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel!.ShotsFired)
                    .Subscribe(_ =>
                    {
                        growAni.RunAsync(ShotsFiredTxt);
                    })
                    .DisposeWith(d);


                this.WhenAnyValue(v => v.ViewModel!.MainMenuVisible)
                    .Subscribe(visible => MenuPanel.Opacity = visible ? 1d : 0d)
                    .DisposeWith(d);
            });
        }
    }
}