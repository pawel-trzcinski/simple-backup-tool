using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class BasicCompressor : Compressor, IBasicCompressor
{
    private readonly ILogger _logger;
    private readonly IZipWrapper _zipWrapper;

    public BasicCompressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IArchiveNameService archiveNameService)
        : base(logger, fileSystemService, zipWrapper, archiveNameService)
    // ReSharper disable once ConvertToPrimaryConstructor
    {
        _logger = logger;
        _zipWrapper = zipWrapper;
    }

    protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun)
    {
        if (compressionType == CompressionType.Adaptive)
        {
            throw new NotSupportedException($"{nameof(BasicCompressor)} does not support  {nameof(CompressionType)}.{compressionType}");
        }

        _logger.Information($"Compressing folder {fileSystemEntity.Source}");
        if (testRun)
        {
            return;
        }

        _zipWrapper.CompressDirectory(zipFile, fileSystemEntity.Source, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType));
    }
}
