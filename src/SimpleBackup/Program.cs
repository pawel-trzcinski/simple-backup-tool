using System.Diagnostics;
using Serilog;
using SimpleBackup.Configuration;
using SimpleBackup.Logging;
using SimpleInjector;
using Microsoft.Extensions.Configuration;
using SimpleBackup.Engine;
using SimpleBackup.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using SimpleBackup.Abstractions;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup
{
    public static class Program
    {
        public static void Main()
        {
            Container? container = null;
            try
            {
                container = new Container();
                RegisterAndVerify(container);
                container.GetInstance<IEngine>().Execute();
            }
            catch (Exception exception)
            {
                Action<string> loggingAction = Console.WriteLine;

                InstanceProducer? producer = container?.GetRegistration<ILogger>();
                if (producer != null)
                {
                    loggingAction = message => ((ILogger)producer.GetInstance()).Fatal(message);
                }

                loggingAction(exception.ToString());
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press <ENTER>");
                Console.ReadLine();
            }
        }

        public static void RegisterAndVerify(Container container)
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            SimpleBackupConfiguration simpleBackupConfiguration = configuration.GetSection(nameof(SimpleBackupConfiguration)).Get<SimpleBackupConfiguration>() ?? throw new ConfigurationMissingException();
            container.RegisterInstance(simpleBackupConfiguration);

            container.RegisterSingleton<ILogger>(() => new LoggerConfiguration()
                .MinimumLevel.Is(simpleBackupConfiguration.LogMinimumLevel)
                .WriteTo.Console(
                    theme: SystemConsoleTheme.Colored,
                    outputTemplate: SeriLogTemplates.GetTemplate(simpleBackupConfiguration.LogMinimumLevel))
                .WriteTo.Debug()
                .CreateLogger());

            container.RegisterSingleton<IDateTimeService, DateTimeService>();
            container.RegisterSingleton<IFileSystemService, FileSystemService>();
            container.RegisterSingleton<IZipWrapper, ZipWrapper>();
            container.RegisterSingleton<IArchiveNameService, ArchiveNameService>();
            container.RegisterSingleton<IArchiveDiskManager, ArchiveDiskManager>();
            container.RegisterSingleton<IThresholdGuard, ThresholdGuard>();

            RegisterPipelineExecutorFactory(container);
            RegisterCompressorFactories(container);

            container.RegisterSingleton<IEngine, Engine.Engine>();

            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        private static void RegisterPipelineExecutorFactory(Container container)
        {
            var producer = Lifestyle.Transient.CreateProducer<IPipelineExecutor, PipelineExecutor>(container);
            container.RegisterInstance<Func<IPipelineExecutor>>(producer.GetInstance);
        }

        private static void RegisterCompressorFactories(Container container)
        {
            var basicProducer = Lifestyle.Transient.CreateProducer<IBasicCompressor, BasicCompressor>(container);
            container.RegisterInstance<Func<IBasicCompressor>>(basicProducer.GetInstance);

            var adaptiveProducer = Lifestyle.Transient.CreateProducer<IAdaptiveCompressor, AdaptiveCompressor>(container);
            container.RegisterInstance<Func<IAdaptiveCompressor>>(adaptiveProducer.GetInstance);

            container.RegisterSingleton<ICompressorFactory, CompressorFactory>();
        }
    }
}
