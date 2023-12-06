using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class AdaptiveCompressor : Compressor, IAdaptiveCompressor
{
    // TODO - manual test with real load

    private readonly IZipWrapper _zipWrapper;

    public AdaptiveCompressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IArchiveNameService archiveNameService)
        : base(logger, fileSystemService, zipWrapper, archiveNameService)
    // ReSharper disable once ConvertToPrimaryConstructor
    {
        _zipWrapper = zipWrapper;
    }

    protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun)
    {
        if (compressionType != CompressionType.Adaptive)
        {
            throw new NotSupportedException($"{nameof(AdaptiveCompressor)} does not support  {nameof(CompressionType)}.{compressionType}");
        }

        if (testRun)
        {
            return;
        }

        _zipWrapper.CompressDirectory(zipFile, fileSystemEntity.Source, file => CompressionLevelDiscoverer.Get(file, CompressionType.Adaptive));
    }
}
