using JetBrains.Annotations;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class BasicCompressor : Compressor, IBasicCompressor
{
    private readonly ILogger _logger;
    private readonly IZipWrapper _zipWrapper;

    [UsedImplicitly]
    public BasicCompressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IThresholdGuard thresholdGuard, IArchiveDiskManager archiveDiskManager)
        : base(logger, fileSystemService, zipWrapper, thresholdGuard, archiveDiskManager)
    // ReSharper disable once ConvertToPrimaryConstructor
    {
        _logger = logger;
        _zipWrapper = zipWrapper;
    }

    protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
    {
        if (compressionType == CompressionType.Adaptive)
        {
            throw new NotSupportedException($"{nameof(BasicCompressor)} does not support  {nameof(CompressionType)}.{compressionType}");
        }

        _logger.Information($"Compressing folder {fileSystemEntity.Source}");
        _zipWrapper.CompressDirectory(zipFile, fileSystemEntity.Source, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType));
    }
}
