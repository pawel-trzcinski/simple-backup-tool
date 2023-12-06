using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Engine;

public class PipelineExecutor(ICompressorFactory compressorFactory) : IPipelineExecutor
{
    public void Execute(BackupPipeline backupPipeline, bool testRun)
    {
        compressorFactory.Create(backupPipeline.Compression).Compress(backupPipeline, testRun);
    }
}
