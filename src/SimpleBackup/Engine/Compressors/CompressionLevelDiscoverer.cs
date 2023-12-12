using System.IO.Compression;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public static class CompressionLevelDiscoverer
{
    private static readonly IReadOnlyDictionary<CompressionType, CompressionLevel> _levelTranslations = new Dictionary<CompressionType, CompressionLevel>
    {
        { CompressionType.Adaptive, CompressionLevel.Optimal },
        { CompressionType.Minimal, CompressionLevel.Fastest },
        { CompressionType.Normal, CompressionLevel.Optimal },
        { CompressionType.Best, CompressionLevel.SmallestSize }
    };

    public static IEnumerable<string> MinimalExtensions { get; } = new string[]
    {
        ".mp3", ".mp4", ".mkv", ".mpg", ".mpeg", ".jpg", ".jpeg", ".png", ".pdf", ".mov", ".zip", ".rar", ".gz", ".7z"
    };

    public static IEnumerable<string> BestExtensions { get; } = new string[]
    {
        ".txt", ".bmp", ".xml", ".svg", ".json", ".js", ".yml", ".yaml", ".html", ".htm", ".tif", ".tiff", ".avi"
    };

    private static readonly Dictionary<string, CompressionType> _extensionsDictionary = CreateExtensionsDictionary();

    private static Dictionary<string, CompressionType> CreateExtensionsDictionary()
    {
        Dictionary<string, CompressionType> result = new Dictionary<string, CompressionType>(StringComparer.OrdinalIgnoreCase);
        foreach (string extension in MinimalExtensions)
        {
            result.Add(extension, CompressionType.Minimal);
        }

        foreach (string extension in BestExtensions)
        {
            result.Add(extension, CompressionType.Best);
        }

        return result;
    }

    public static CompressionLevel Get(string source, CompressionType compressionType)
    {
        return compressionType == CompressionType.Adaptive ? _levelTranslations[RecognizeType(source)] : _levelTranslations[compressionType];
    }

    private static CompressionType RecognizeType(string source)
    {
        string extension = Path.GetExtension(source);
        return _extensionsDictionary.TryGetValue(extension, out CompressionType compressionType) ? compressionType : CompressionType.Normal;
    }
}
