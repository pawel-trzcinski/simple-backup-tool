using Serilog;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class CompressorFactory(ILogger log, Func<IBasicCompressor> basicCompressorFactory, Func<IAdaptiveCompressor> adaptiveCompressorFactory)
    : ICompressorFactory
{
    // TODO - unit tests

    public ICompressor Create(CompressionType compressionLevel)
    {
        switch (compressionLevel)
        {
            case CompressionType.Adaptive:
                log.Information("Using adaptive compression.");
                return adaptiveCompressorFactory();
            case CompressionType.Minimal:
            case CompressionType.Normal:
            case CompressionType.Best:
                log.Information("Using basic compression.");
                return basicCompressorFactory();
            default:
                throw new ArgumentOutOfRangeException(nameof(compressionLevel), compressionLevel, null);
        }
    }
}
