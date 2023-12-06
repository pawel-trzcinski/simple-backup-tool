namespace SimpleBackup.Engine.Compressors
{
    public interface IArchiveNameService
    {
        public const string FINISHED = nameof(FINISHED);

        string ConstructArchiveFolderName();

        TimeSpan GetTimePassedFromLatesFinishedArchive(string mainFolder);
    }
}