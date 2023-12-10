using SimpleBackup.Configuration;

namespace SimpleBackup.Engine;

public interface IPipelineExecutor
{
    void Execute(BackupPipeline backupPipeline);
}