using System.IO.Compression;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public class BasicCompressor : Compressor, IBasicCompressor
{
    private readonly ILogger _logger;

    public BasicCompressor(ILogger logger, IFileSystemService fileSystemService, IDateTimeService dateTimeService)
        : base(logger, fileSystemService, dateTimeService)
    // ReSharper disable once ConvertToPrimaryConstructor
    {
        _logger = logger;
    }

    protected override void CompressFile(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun)
    {
        if (compressionType == CompressionType.Adaptive)
        {
            throw new InvalidOperationException($"{nameof(BasicCompressor)} does not support  {nameof(CompressionType)}.{compressionType}");
        }

        _logger.Information($"Compressing file {fileSystemEntity.Source}");
        if (!testRun)
        {
            using (var fileStream = new FileStream(zipFile, FileMode.Create))
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                zipArchive.CreateEntryFromFile(fileSystemEntity.Source, fileSystemEntity.Name, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType));
            }
        }
    }

    protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun)
    {
        if (compressionType == CompressionType.Adaptive)
        {
            throw new InvalidOperationException($"{nameof(BasicCompressor)} does not support  {nameof(CompressionType)}.{compressionType}");
        }

        _logger.Information($"Compressing folder {fileSystemEntity.Source}");
        if (!testRun)
        {
            ZipFile.CreateFromDirectory(fileSystemEntity.Source, zipFile, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType), true);
        }
    }
}
