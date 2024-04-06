using Avalonia;
using AvaStride;
using FirstPersonShooter.UI;
using Stride.Core;
using Stride.Core.Presentation.Interop;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using System;

namespace FirstPersonShooter;
public class CustomGame : Game
{
	protected override void BeginRun()
	{
		base.BeginRun();

		WindowMinimumUpdateRate.MinimumElapsedTime = TimeSpan.Zero;
		MinimizedMinimumUpdateRate.MinimumElapsedTime = TimeSpan.Zero;
		Window.IsBorderLess = true;
        Window.Position = new Stride.Core.Mathematics.Int2(0, 0);

		var appBuilder = Program.BuildAvaloniaApp();
        AvaloniaInStride.StartAndAttachAvalonia(this, appBuilder);
    }
}
