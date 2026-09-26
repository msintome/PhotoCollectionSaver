using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace PhotoCollectionSaver;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
        {
            var library = new PhotoLibrary(PhotoLibrary.DefaultFolder);

            // Avalonia only exposes the list of screens through a window, so the first
            // window is created before we know which screen it goes on.
            var first = new SlideshowWindow(library);
            IReadOnlyList<Screen> screens = first.Screens.All;

            if (screens.Count == 0)
            {
                first.Show();
            }
            else
            {
                first.ShowOn(screens[0]);
                foreach (Screen screen in screens.Skip(1))
                    new SlideshowWindow(library).ShowOn(screen);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
