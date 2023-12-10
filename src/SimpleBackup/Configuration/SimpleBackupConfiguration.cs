using Serilog.Events;

namespace SimpleBackup.Configuration;

public sealed class SimpleBackupConfiguration
{
    public IReadOnlyCollection<BackupPipeline> BackupPipelines { get; init; } = Array.Empty<BackupPipeline>();

    public LogEventLevel LogMinimumLevel { get; init; } = LogEventLevel.Information;
}
