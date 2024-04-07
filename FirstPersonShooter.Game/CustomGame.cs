using AvaStride;
using FirstPersonShooter.UI;
using Stride.Engine;
using Stride.Graphics;
using Stride.Core.Mathematics;
using System;

namespace FirstPersonShooter;
public class CustomGame : Game
{
	protected override void BeginRun()
	{
		base.BeginRun();

		WindowMinimumUpdateRate.MinimumElapsedTime = TimeSpan.Zero;
		MinimizedMinimumUpdateRate.MinimumElapsedTime = TimeSpan.Zero;

		var bounds = GraphicsDevice.Adapter.Outputs[0].DesktopBounds;
		
		Window.SetSize(new Int2(bounds.Width, bounds.Height));
		Window.Position = new Int2(0, 0);
		Window.IsBorderLess = true;

		var appBuilder = Program.BuildAvaloniaApp();
        AvaloniaInStride.StartAndAttachAvalonia(this, appBuilder);
    }
}
