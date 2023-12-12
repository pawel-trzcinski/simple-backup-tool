namespace SimpleBackup.Engine.Compressors
{
    public interface IThresholdGuard
    {
        bool RefreshThresholdPassed(string mainFolder, TimeSpan threshold);
    }
}
