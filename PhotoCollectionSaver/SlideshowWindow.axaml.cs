using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace PhotoCollectionSaver;

/// <summary>
/// A full-screen slideshow on one monitor. Any key, click or mouse movement exits the whole app.
/// </summary>
public partial class SlideshowWindow : Window
{
    private static readonly TimeSpan SlideInterval = TimeSpan.FromSeconds(10);

    // Going full screen can move the window under a stationary mouse, which would look
    // like the user moving it, so pointer movement is ignored for a moment after opening.
    private static readonly TimeSpan InputGracePeriod = TimeSpan.FromSeconds(1);

    private const int MouseMoveTolerance = 5;
    private const int MaxLoadAttempts = 5;

    private readonly PhotoLibrary _library;
    private readonly DispatcherTimer _timer;
    private bool _closed;
    private DateTime _inputArmedAt;
    private PixelPoint? _mouseStart;

    // The previous photo is kept alive while it cross-fades out.
    private Bitmap? _current;
    private Bitmap? _previous;

    // For the XAML designer.
    public SlideshowWindow() : this(new PhotoLibrary(PhotoLibrary.DefaultFolder)) { }

    public SlideshowWindow(PhotoLibrary library)
    {
        InitializeComponent();
        _library = library;

        _timer = new DispatcherTimer { Interval = SlideInterval };
        _timer.Tick += (_, _) => ShowNextPhoto();

        Cursor = new Cursor(StandardCursorType.None);
        // On macOS a topmost window can't go full screen, and doesn't need to.
        Topmost = OperatingSystem.IsWindows();

        Opened += OnOpened;
        Closed += OnClosed;
    }

    public void ShowOn(Screen screen)
    {
        Position = screen.Bounds.Position;
        WindowState = WindowState.FullScreen;
        Show();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _inputArmedAt = DateTime.UtcNow + InputGracePeriod;

        if (_library.IsEmpty)
        {
            Message.Text = $"No photos found in {_library.Folder}";
            Message.IsVisible = true;
            return;
        }

        ShowNextPhoto();
        _timer.Start();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _closed = true;
        _timer.Stop();
        _current?.Dispose();
        _previous?.Dispose();
    }

    private async void ShowNextPhoto()
    {
        Bitmap? bitmap = null;
        for (int attempt = 0; bitmap is null && attempt < MaxLoadAttempts; attempt++)
        {
            string path = _library.NextPath();
            // Screen sizes are in points on macOS but pixels on Windows, so use the window's own scale.
            int height = (int)Math.Ceiling(Bounds.Height * RenderScaling);
            bitmap = await Task.Run(() => TryLoad(path, height));
        }

        if (bitmap is null)
            return;
        if (_closed)
        {
            bitmap.Dispose();
            return;
        }

        _previous?.Dispose();
        _previous = _current;
        _current = bitmap;
        Slide.Content = new Image { Source = bitmap, Stretch = Stretch.Uniform };
    }

    // Decodes at screen height rather than full size, to keep memory down with large photos.
    private static Bitmap? TryLoad(string path, int height)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            return height > 0 ? Bitmap.DecodeToHeight(stream, height) : new Bitmap(stream);
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) => Exit();

    protected override void OnPointerPressed(PointerPressedEventArgs e) => Exit();

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        PixelPoint position = this.PointToScreen(e.GetPosition(this));
        if (_mouseStart is not PixelPoint start || DateTime.UtcNow < _inputArmedAt)
        {
            _mouseStart = position;
            return;
        }

        if (Math.Abs(position.X - start.X) > MouseMoveTolerance ||
            Math.Abs(position.Y - start.Y) > MouseMoveTolerance)
            Exit();
    }

    private static void Exit() =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
}
