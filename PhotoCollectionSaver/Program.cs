using Avalonia;

namespace PhotoCollectionSaver;

internal enum LaunchMode { Show, Preview, Configure, Unknown }

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        switch (ParseMode(args))
        {
            case LaunchMode.Show:
                return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            case LaunchMode.Preview:
                // TODO: draw into the Windows preview box (its window handle is in the arguments).
                return 0;

            case LaunchMode.Configure:
                // TODO: settings window.
                return 0;

            default:
                return 1;
        }
    }

    // Used by the Avalonia designer as well as Main.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    // Windows launches screensavers with /s (show), /p <hwnd> (preview) or /c (configure),
    // in any case and optionally as /c:<hwnd>. No arguments means "configure".
    // macOS has no screensaver host for this app, so no arguments starts the slideshow there.
    internal static LaunchMode ParseMode(string[] args)
    {
        if (args.Length == 0)
            return OperatingSystem.IsWindows() ? LaunchMode.Configure : LaunchMode.Show;

        string arg = args[0].Trim().ToLowerInvariant();
        if (arg.Length < 2 || (arg[0] != '/' && arg[0] != '-') || (arg.Length > 2 && arg[2] != ':'))
            return LaunchMode.Unknown;

        return arg[1] switch
        {
            's' => LaunchMode.Show,
            'p' => LaunchMode.Preview,
            'c' => LaunchMode.Configure,
            _ => LaunchMode.Unknown,
        };
    }
}
