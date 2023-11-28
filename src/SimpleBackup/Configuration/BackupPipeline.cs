namespace SimpleBackup.Configuration;

public sealed class BackupPipeline
{
    public string Name { get; init; }

    public bool Enabled { get; init; }

    /// <summary>
    /// Collection of files or folders to be added to archive.
    /// </summary>
    public IReadOnlyCollection<string> Sources { get; init; }

    public CompressionType Compression { get; init; }

    /// <summary>
    /// Folder that backup will be saved to.
    /// </summary>
    public string BackupOutputFolder { get; init; }

    // TODO - sprawdzamy top folder, czy są starsze pliki z tą samą nazwą albo z template
    public bool RemoveOldArchive { get; init; }
}
