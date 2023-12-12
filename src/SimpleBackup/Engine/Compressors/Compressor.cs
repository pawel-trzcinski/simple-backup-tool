using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public abstract class Compressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IThresholdGuard thresholdGuard, IArchiveDiskManager archiveDiskManager) : ICompressor
{
    public void Compress(BackupPipeline backupPipeline)
    {
        string mainFolder = archiveDiskManager.PrepareMainFolder(backupPipeline.BackupOutputFolder, backupPipeline.Name);

        if (!thresholdGuard.RefreshThresholdPassed(mainFolder, backupPipeline.RefreshThreshold))
        {
            logger.Information($"Refresh threshold not passed for {backupPipeline.Name}. Ignoring");
            return;
        }

        string archiveFolder = archiveDiskManager.PrepareArchiveFolder(mainFolder);

        logger.Information($"Compressing {backupPipeline.Name} into {archiveFolder}");

        int index = 0;
        foreach (string source in backupPipeline.Sources)
        {
            FileSystemEntity fileSystemEntity = fileSystemService.RecognizeEntity(source);
            string zipFile = archiveDiskManager.PrepareEntryFolder(archiveFolder, fileSystemEntity, index);

            if (fileSystemEntity.Type == FileSystemEntityType.File)
            {
                CompressFile(fileSystemEntity, zipFile, backupPipeline.Compression);
            }
            else
            {
                CompressDirectory(fileSystemEntity, zipFile, backupPipeline.Compression);
            }

            ++index;
        }

        logger.Information($"All sources compressed for archive {backupPipeline.Name}");
        string newArchiveFolder = archiveDiskManager.FinishArchive(archiveFolder);

        if (backupPipeline.RemoveOldArchive)
        {
            logger.Information("Removing old archives");
            fileSystemService.RemoveAllDirectoriesExcept(mainFolder, newArchiveFolder);
        }
    }

    private void CompressFile(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
    {
        logger.Information($"Compressing file {fileSystemEntity.Source}");
        zipWrapper.CompressFile(zipFile, fileSystemEntity.Source, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType));
    }

    protected abstract void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType);
}
