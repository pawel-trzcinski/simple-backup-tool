using Serilog.Events;

namespace SimpleBackup.Configuration;

public sealed class SimpleBackupConfiguration
{
    public IReadOnlyCollection<BackupPipeline> BackupPipelines { get; init; } = Array.Empty<BackupPipeline>();

    public LogEventLevel LogMinimumLevel { get; init; } = LogEventLevel.Information;

    /// <summary>
    /// If enabled, application will do everything except changing data (this includes saving new files, compressing etc.).
    /// </summary>
    public bool TestRun { get; init; }
}
