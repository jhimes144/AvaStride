using AvaStride;
using FirstPersonShooter.UI;
using Stride.Engine;
using Stride.Graphics;
using Stride.Core.Mathematics;
using System;
using Stride.Games;

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
        Window.IsBorderLess = true;
        Window.Position = new Int2(0, 0);


		var appBuilder = Program.BuildAvaloniaApp();
        AvaloniaInStride.StartAndAttachAvalonia(this, appBuilder);

		// this must be added after avalonia is attached.
        Services.AddService(new MainMenuService(this));
    }
}
