using System.Diagnostics;
using Serilog;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine;

public class Engine(ILogger logger, SimpleBackupConfiguration configuration, Func<IPipelineExecutor> pipelineExecutorFactory)
    : IEngine
{
    public void Execute()
    {
        logger.Information($"{nameof(SimpleBackup)} started");

        foreach (BackupPipeline pipeline in configuration.BackupPipelines)
        {
            if (!pipeline.Enabled)
            {
                logger.Information($"Pipeline {pipeline.Name} disabled");
                continue;
            }

            var stopwatch = Stopwatch.StartNew();

            logger.Information($"Started {pipeline.Name}");
            pipelineExecutorFactory().Execute(pipeline, configuration.TestRun);

            stopwatch.Stop();
            logger.Information($"Finished {pipeline.Name}. Elapsed: {stopwatch.Elapsed:g}");
        }
    }
}
