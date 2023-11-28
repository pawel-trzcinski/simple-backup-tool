using System.Diagnostics;
using Serilog;
using SimpleBackup.Configuration;

namespace SimpleBackup.Engine;

public class Engine(ILogger log, SimpleBackupConfiguration configuration, Func<IPipelineExecutor> pipelineExecutorFactory)
    : IEngine
{
    // TODO - unit tests
    
    public void Execute()
    {
        log.Information($"{nameof(SimpleBackup)} started");

        foreach (BackupPipeline pipeline in configuration.BackupPipelines)
        {
            var stopwatch = Stopwatch.StartNew();

            log.Information($"Started {pipeline.Name}");
            pipelineExecutorFactory().Execute(pipeline, configuration.TestRun);

            stopwatch.Stop();
            log.Information($"Finished {pipeline.Name}. Elapsed: {stopwatch.Elapsed:g}");
        }
    }
}
