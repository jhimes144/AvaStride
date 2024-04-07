# AvaStride

### [NuGet](https://www.nuget.org/packages/AvaStride/1.0.0/) [![NuGet](https://img.shields.io/nuget/v/AvaStride?color=blue)](https://www.nuget.org/packages/AvaStride/1.0.0/)

## What does this library do?

AvaStride allows you to run an [Avalonia](https://avaloniaui.net/)11x app inside [Stride](https://www.stride3d.net/), to be used as your game/project's UI. Technically speaking, it layers Avalonia on top of Stride via low-level win32 apis. Avalonia is ran on a seperate thread from Stride's main thread, and so this library also provides mechanisms for communicating to and from Stride and Avalonia. This library, at time of writing works with the latest Stride and Avalonia (v11x) versions.

## Why use this library?

Stride is an amazing open-source game engine, but like many game engines - the UI implementation can be a bit rough. Creating a solid UI framework is an enormous endeavor. Avalonia is one of the most popular open source .Net UI frameworks out there. It is well tested, stable, and extremely feature rich. This library is great for games that plan on heavily relying on UI or developers who have a desktop app dev background.

## How well does it work?

AvaStride is able to run both Avalonia and Stride at their native performance. There isn't any CPU buffering of textures. It should also be pretty easy to stay in step with the latest versions of both Stride and Avalonia, since this library doesn't rely too much on the inner intricacies of either. However, we have the following limitations:

- This library only works on Windows
- Exclusive fullscreen mode is unavailable (fullscreen is still available).
- Window bluring and mica are not supported (transparency is).
- Overlays like the steam overlay draw in-between Avalonia and your Stride game/project. Steam's api does provide the following [callback](https://partner.steamgames.com/doc/api/ISteamFriends#GameOverlayActivated_t) for when the overlay is activated, allowing you to hide the UI in response to the overlay becoming active.

Note: Exclusive fullscreen is when a game bypasses Window's layering system and draws directly to the GPU. In many cases, this yields a small performance advantage.

## Mouse and Keyboard
To keep things simple, AvaStride allows you to enable either the game or the ui for m/k events at any given time but not both at the same time. Note: There are some cases where Stride can capture input aside from its Window, so this must be accounted for as well. Avalonia is perfectly capable of capturing mouse and keyboard explicitly, for you to forward to Stride in the manner you wish. The reverse isn't as easy.

## Getting Started

### Working Sample
![alt text](https://github.com/jhimes144/AvaStride/blob/main/FirstPersonShooter.Windows/sampleShot.png?raw=true)
Inside this repo is a sample based on an [improved FPS template](https://github.com/Doprez/smooth-fps-template/tree/main).

### Existing/New Project
1. Install the AvaStride nuget package to your Stride solution.
2. Install [Avalonia Dotnet Templates](https://github.com/AvaloniaUI/avalonia-dotnet-templates)
3. Create a new Avalonia app in your Stride solution `dotnet new avalonia.app -o MyApp`
4. In `App.axaml.cs` modify `OnFrameworkInitializationCompleted` to the following:
```
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

    base.OnFrameworkInitializationCompleted();
}
```
5. Change `Program.cs` so that its public, with `BuildAvaloniaApp()` public as well.
6. Have your Stride project reference your new Avalonia App.
7. In your Game class code, as part of your game initialization, add the following code:
```
var appBuilder = Program.BuildAvaloniaApp();
AvaloniaInStride.StartAndAttachAvalonia(this, appBuilder);
```
8. Run your game. You should see Welcome To Avalonia! displayed.

#### Order of operations
The above code starts with your game initialization. `AvaloniaInStride.StartAndAttachAvalonia` is called, and builds your Avalonia app. Once the app is built `AvaloniaInStride.InitializeWithWindow` is called to attach the Avalonia window to Stride. Meanwhile, `AvaloniaInStride.StartAndAttachAvalonia` blocks until the window is loaded and ready.

Note: Your Avalonia app can still be ran outside of the Game by itself, provided you write your user code in such a way where it can do so without the game running.