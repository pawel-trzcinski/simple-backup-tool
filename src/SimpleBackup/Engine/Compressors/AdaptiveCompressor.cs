using JetBrains.Annotations;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class AdaptiveCompressor : Compressor, IAdaptiveCompressor
{
    private readonly IZipWrapper _zipWrapper;

    [UsedImplicitly]
    public AdaptiveCompressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IThresholdGuard thresholdGuard, IArchiveDiskManager archiveDiskManager)
        : base(logger, fileSystemService, zipWrapper, thresholdGuard, archiveDiskManager)
    // ReSharper disable once ConvertToPrimaryConstructor
    {
        _zipWrapper = zipWrapper;
    }

    protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
    {
        if (compressionType != CompressionType.Adaptive)
        {
            throw new NotSupportedException($"{nameof(AdaptiveCompressor)} does not support  {nameof(CompressionType)}.{compressionType}");
        }

        _zipWrapper.CompressDirectory(zipFile, fileSystemEntity.Source, file => CompressionLevelDiscoverer.Get(file, CompressionType.Adaptive));
    }
}
