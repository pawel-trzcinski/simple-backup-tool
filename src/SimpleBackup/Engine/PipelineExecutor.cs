using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Engine;

public class PipelineExecutor(ICompressorFactory compressorFactory) : IPipelineExecutor
{
    public void Execute(BackupPipeline backupPipeline)
    {
        compressorFactory.Create(backupPipeline.Compression).Compress(backupPipeline);
    }
}
