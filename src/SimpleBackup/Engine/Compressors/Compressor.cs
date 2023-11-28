using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public abstract class Compressor(ILogger logger, IFileSystemService fileSystemService, IDateTimeService dateTimeService) : ICompressor
{
    // TODO - unit tests

    private const string ARCHIVE = nameof(ARCHIVE);
    private const string ENTRY = nameof(ENTRY);
    private const string SOURCE = nameof(SOURCE);

    public void Compress(BackupPipeline backupPipeline, bool testRun)
    {
        // desFolder/name/Entry

        string mainFolder = PrepareMainFolder(backupPipeline.BackupOutputFolder, backupPipeline.Name, testRun);
        string archiveFolder = PrepareArchiveFolder(mainFolder, testRun);

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
        }
    }

    protected abstract void CompressFile(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionLevel, bool testRun);
    protected abstract void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionLevel, bool testRun);

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
        string archiveFolder = Path.Combine(mainFolder, $"{ARCHIVE}_{dateTimeService.Now:yyyy_MM_dd_HH_mm_ss}");

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

        string entrySourceIndicatorFile = Path.Combine(entryFolder, $"{fileSystemEntity.Name}.{SOURCE}");
        logger.Information($"Creating entry source indicator file {entrySourceIndicatorFile}");
        if (!testRun)
        {
            fileSystemService.WriteTextToFile(entrySourceIndicatorFile, fileSystemEntity.Source);
        }

        return Path.Combine(entryFolder, $"{fileSystemEntity.Name}.zip");
    }
}
