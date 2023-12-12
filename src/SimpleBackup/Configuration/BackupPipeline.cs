namespace SimpleBackup.Configuration;

public sealed class BackupPipeline
{
    public string Name { get; init; } = String.Empty;

    public bool Enabled { get; init; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public TimeSpan RefreshThreshold { get; init; }

    /// <summary>
    /// Collection of files or folders to be added to archive.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IReadOnlyCollection<string> Sources { get; init; } = Array.Empty<string>();

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public CompressionType Compression { get; init; }

    /// <summary>
    /// Folder that backup will be saved to.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string BackupOutputFolder { get; init; } = String.Empty;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool RemoveOldArchive { get; init; }
}
