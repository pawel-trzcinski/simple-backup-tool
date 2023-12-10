using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public interface ICompressor
{
    void Compress(BackupPipeline backupPipeline);
}
