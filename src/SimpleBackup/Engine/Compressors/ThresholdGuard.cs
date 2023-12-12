namespace SimpleBackup.Engine.Compressors
{
    public class ThresholdGuard(IArchiveNameService archiveNameService) : IThresholdGuard
    {
        public bool RefreshThresholdPassed(string mainFolder, TimeSpan threshold)
        {
            return archiveNameService.GetTimePassedFromLatesFinishedArchive(mainFolder) > threshold;
        }
    }
}
