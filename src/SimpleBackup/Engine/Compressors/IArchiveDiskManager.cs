using SimpleBackup.Abstractions;

namespace SimpleBackup.Engine.Compressors
{
    public interface IArchiveDiskManager
    {
        string PrepareMainFolder(string backupOutputFolder, string pipelineName);

        /// <returns>Name of the destination zip file.</returns>
        string PrepareArchiveFolder(string mainFolder);

        /// <summary>
        /// Creates folder for single source from pipeline.
        /// </summary>
        /// <param name="fileSystemEntity" />
        /// <param name="index">Index of source in source array.</param>
        /// <param name="archiveFolder" />
        /// <returns>Name of the zip file in entry folder.</returns>
        string PrepareEntryFolder(string archiveFolder, FileSystemEntity fileSystemEntity, int index);

        /// <summary>
        /// Marks archive folder as finished.
        /// </summary>
        /// <param name="archiveFolder" />
        /// <returns>New path to archive folder.</returns>
        string FinishArchive(string archiveFolder);
    }
}
