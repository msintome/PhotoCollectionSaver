namespace PhotoCollectionSaver;

/// <summary>
/// The photos in one folder, handed out in a shuffled order that reshuffles
/// once every photo has been shown.
/// </summary>
public sealed class PhotoLibrary
{
    // Formats Avalonia can decode. HEIC (the iPhone default) isn't one of them.
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };

    private readonly string[] _paths;
    private int _next;

    public PhotoLibrary(string folder)
    {
        Folder = folder;
        _paths = FindPhotos(folder);
        Random.Shared.Shuffle(_paths);
    }

    // TODO: replace with the folder from settings once they exist.
    public static string DefaultFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    public string Folder { get; }

    public bool IsEmpty => _paths.Length == 0;

    public string NextPath()
    {
        if (_next == _paths.Length)
        {
            Random.Shared.Shuffle(_paths);
            _next = 0;
        }
        return _paths[_next++];
    }

    private static string[] FindPhotos(string folder)
    {
        try
        {
            return Directory.EnumerateFiles(folder)
                .Where(path => SupportedExtensions.Contains(Path.GetExtension(path)))
                .ToArray();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }
}
