using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public abstract class Compressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IArchiveNameService archiveNameService) : ICompressor
{
    // TODO - unit tests

    private const string ENTRY = nameof(ENTRY);
    private const string SOURCE = nameof(SOURCE);

    public void Compress(BackupPipeline backupPipeline)
    {
        // TODO - IArchiveFolder
        string mainFolder = PrepareMainFolder(backupPipeline.BackupOutputFolder, backupPipeline.Name);

        // TODO - IThresholdGuard
        if (!RefreshThresholdPassed(mainFolder, backupPipeline.RefreshThreshold))
        {
            logger.Information($"Refresh threshold not passed for {backupPipeline.Name}. Ignoring");
            return;
        }

        // TODO - IArchiveFolder
        string archiveFolder = PrepareArchiveFolder(mainFolder);

        logger.Information($"Compressing {backupPipeline.Name} into {archiveFolder}");

        int index = 0;
        foreach (string source in backupPipeline.Sources)
        {
            FileSystemEntity fileSystemEntity = fileSystemService.RecognizeEntity(source);
            // TODO - IArchiveFolder
            string zipFile = PrepareEntryFolder(archiveFolder, fileSystemEntity, index);

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

        string newArchiveFolder = $"{archiveFolder}.{IArchiveNameService.FINISHED}";
        fileSystemService.MoveDirectory(archiveFolder, newArchiveFolder);

        if (backupPipeline.RemoveOldArchive)
        {
            logger.Information("Removing old archives");
            fileSystemService.RemoveAllDirectoriesExcept(mainFolder, newArchiveFolder);
        }
    }

    private bool RefreshThresholdPassed(string mainFolder, TimeSpan threshold)
    {
        return archiveNameService.GetTimePassedFromLatesFinishedArchive(mainFolder) > threshold;
    }

    private void CompressFile(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
    {
        logger.Information($"Compressing file {fileSystemEntity.Source}");
        zipWrapper.CompressFile(zipFile, fileSystemEntity.Source, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType));
    }

    protected abstract void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType);

    private string PrepareMainFolder(string backupOutputFolder, string pipelineName)
    {
        string mainFolder = Path.Combine(backupOutputFolder, pipelineName);

        if (!fileSystemService.DirectoryExists(mainFolder))
        {
            logger.Information($"Creating directory {mainFolder}");
            fileSystemService.CreateDirectory(mainFolder);
        }

        return mainFolder;
    }

    /// <returns>Name of the destination zip file.</returns>
    private string PrepareArchiveFolder(string mainFolder)
    {
        string archiveFolder = Path.Combine(mainFolder, archiveNameService.ConstructArchiveFolderName());

        if (fileSystemService.DirectoryExists(archiveFolder))
        {
            throw new InvalidOperationException($"Archive directory already exists - {archiveFolder}");
        }

        logger.Information($"Creating archive directory {archiveFolder}");
        fileSystemService.CreateDirectory(archiveFolder);

        return archiveFolder;
    }

    private string PrepareEntryFolder(string archiveFolder, FileSystemEntity fileSystemEntity, int index)
    {
        string entryFolder = Path.Combine(archiveFolder, $"{ENTRY}{index:000}");

        logger.Information($"Creating entry folder {entryFolder}");
        fileSystemService.CreateDirectory(entryFolder);

        string entityName = Path.GetFileName(fileSystemEntity.Source);

        string entrySourceIndicatorFile = Path.Combine(entryFolder, $"{entityName}.{SOURCE}");
        logger.Information($"Creating entry source indicator file {entrySourceIndicatorFile}");
        fileSystemService.WriteTextToFile(entrySourceIndicatorFile, fileSystemEntity.Source);

        return Path.Combine(entryFolder, $"{entityName}.zip");
    }
}
