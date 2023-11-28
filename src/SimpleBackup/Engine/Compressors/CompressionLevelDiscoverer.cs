using System.IO.Compression;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public static class CompressionLevelDiscoverer
{
    // TODO - unit tests

    // TODO - test, że wszystkie klucze są
    private static readonly IReadOnlyDictionary<CompressionType, CompressionLevel> _levelTranslations = new Dictionary<CompressionType, CompressionLevel>
    {
        { CompressionType.Adaptive, CompressionLevel.Optimal },
        { CompressionType.Minimal, CompressionLevel.Fastest },
        { CompressionType.Normal, CompressionLevel.Optimal },
        { CompressionType.Best, CompressionLevel.SmallestSize }
    };

    public static CompressionLevel Get(string source, CompressionType compressionType)
    {
        return compressionType == CompressionType.Adaptive ? RecognizeLevel(source) : _levelTranslations[compressionType];
    }

    private static CompressionLevel RecognizeLevel(string source)
    {
        // TODO

        return CompressionLevel.Optimal;
    }
}
