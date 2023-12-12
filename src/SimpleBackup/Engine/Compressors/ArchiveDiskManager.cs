using Serilog;
using SimpleBackup.Abstractions;

namespace SimpleBackup.Engine.Compressors
{
    public class ArchiveDiskManager(ILogger logger, IFileSystemService fileSystemService, IArchiveNameService archiveNameService) : IArchiveDiskManager
    {
        public const string ENTRY = nameof(ENTRY);
        public const string SOURCE = nameof(SOURCE);

        public string PrepareMainFolder(string backupOutputFolder, string pipelineName)
        {
            string mainFolder = Path.Combine(backupOutputFolder, pipelineName);

            if (!fileSystemService.DirectoryExists(mainFolder))
            {
                logger.Information($"Creating directory {mainFolder}");
                fileSystemService.CreateDirectory(mainFolder);
            }

            return mainFolder;
        }

        public string PrepareArchiveFolder(string mainFolder)
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

        public string PrepareEntryFolder(string archiveFolder, FileSystemEntity fileSystemEntity, int index)
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

        public string FinishArchive(string archiveFolder)
        {
            string newArchiveFolder = $"{archiveFolder}.{IArchiveNameService.FINISHED}";
            fileSystemService.MoveDirectory(archiveFolder, newArchiveFolder);

            return newArchiveFolder;
        }
    }
}
