using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public abstract class Compressor(ILogger logger, IFileSystemService fileSystemService, IZipWrapper zipWrapper, IArchiveNameService archiveNameService) : ICompressor
{
    // TODO - unit tests

    private const string ENTRY = nameof(ENTRY);
    private const string SOURCE = nameof(SOURCE);
    
    public void Compress(BackupPipeline backupPipeline, bool testRun)
    {
        // desFolder/Name/ARCHIVE_yyyy
        // desFolder/mainFolder/archiveFolder

        string mainFolder = PrepareMainFolder(backupPipeline.BackupOutputFolder, backupPipeline.Name, testRun);

        if (!testRun && !RefreshThresholdPassed(mainFolder, backupPipeline.RefreshThreshold))
        {
            logger.Information($"Refresh threshold not passed for {backupPipeline.Name}. Ignoring");
            return;
        }

        string archiveFolder = PrepareArchiveFolder(mainFolder, testRun);

        logger.Information($"Compressing {backupPipeline.Name} into {archiveFolder}");

        int index = 0;
        foreach (string source in backupPipeline.Sources)
        {
            FileSystemEntity fileSystemEntity = fileSystemService.RecognizeEntity(source);
            string zipFile = PrepareEntryFolder(archiveFolder, fileSystemEntity, index, testRun);

            if (fileSystemEntity.Type == FileSystemEntityType.File)
            {
                CompressFile(fileSystemEntity, zipFile, backupPipeline.Compression, testRun);
            }
            else
            {
                CompressDirectory(fileSystemEntity, zipFile, backupPipeline.Compression, testRun);
            }

            ++index;
        }

        logger.Information($"All sources compressed for archive {backupPipeline.Name}");
        // TODO - manual test
        fileSystemService.MoveDirectory(archiveFolder, $"{archiveFolder}.{IArchiveNameService.FINISHED}");

        if (backupPipeline.RemoveOldArchive)
        {
            logger.Information("Removing old archives");
            if (!testRun)
            {
                fileSystemService.RemoveAllDirectoriesExcept(mainFolder, archiveFolder);
            }
        }
    }

    private bool RefreshThresholdPassed(string mainFolder, TimeSpan threshold)
    {
        return archiveNameService.GetTimePassedFromLatesFinishedArchive(mainFolder) > threshold;
    }

    private void CompressFile(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun)
    {
        logger.Information($"Compressing file {fileSystemEntity.Source}");
        if (!testRun)
        {
            zipWrapper.CompressFile(zipFile, fileSystemEntity.Source, CompressionLevelDiscoverer.Get(fileSystemEntity.Source, compressionType));
        }
    }

    protected abstract void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun);

    private string PrepareMainFolder(string backupOutputFolder, string pipelineName, bool testRun)
    {
        string mainFolder = Path.Combine(backupOutputFolder, pipelineName);

        if (!fileSystemService.DirectoryExists(mainFolder))
        {
            logger.Information($"Creating directory {mainFolder}");
            if (!testRun)
            {
                fileSystemService.CreateDirectory(mainFolder);
            }
        }

        return mainFolder;
    }

    /// <returns>Name of the destination zip file.</returns>
    private string PrepareArchiveFolder(string mainFolder, bool testRun)
    {
        string archiveFolder = Path.Combine(mainFolder, archiveNameService.ConstructArchiveFolderName());

        if (fileSystemService.DirectoryExists(archiveFolder))
        {
            throw new InvalidOperationException($"Archive directory already exists - {archiveFolder}");
        }

        logger.Information($"Creating archive directory {archiveFolder}");
        if (!testRun)
        {
            fileSystemService.CreateDirectory(archiveFolder);
        }

        return archiveFolder;
    }

    private string PrepareEntryFolder(string archiveFolder, FileSystemEntity fileSystemEntity, int index, bool testRun)
    {
        string entryFolder = Path.Combine(archiveFolder, $"{ENTRY}{index:000}");

        logger.Information($"Creating entry folder {entryFolder}");
        if (!testRun)
        {
            fileSystemService.CreateDirectory(entryFolder);
        }

        string entityName = Path.GetFileName(fileSystemEntity.Source);

        string entrySourceIndicatorFile = Path.Combine(entryFolder, $"{entityName}.{SOURCE}");
        logger.Information($"Creating entry source indicator file {entrySourceIndicatorFile}");
        if (!testRun)
        {
            fileSystemService.WriteTextToFile(entrySourceIndicatorFile, fileSystemEntity.Source);
        }

        return Path.Combine(entryFolder, $"{entityName}.zip");
    }
}
