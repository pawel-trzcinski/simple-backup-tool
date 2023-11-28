using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class AdaptiveCompressor : Compressor, IAdaptiveCompressor
{
    // TODO - unit tests

    public AdaptiveCompressor(ILogger logger, IFileSystemService fileSystemService, IDateTimeService dateTimeService)
        : base(logger, fileSystemService, dateTimeService)
    // ReSharper disable once ConvertToPrimaryConstructor
    {
    }

    protected override void CompressFile(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionLevel, bool testRun)
    {
        // TODO
    }

    protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionLevel, bool testRun)
    {
        // TODO
    }
}
