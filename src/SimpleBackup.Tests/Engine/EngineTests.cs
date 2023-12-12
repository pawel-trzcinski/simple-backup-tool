using AutoFixture;
using Combinatorics.Collections;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Configuration;
using SimpleBackup.Engine;

namespace SimpleBackup.Tests.Engine
{
    [TestFixture]
    public class EngineTests
    {
        private static readonly ILogger _logger = Substitute.For<ILogger>();
        private Func<IPipelineExecutor> _pipelineExecutorFactory = Substitute.For<Func<IPipelineExecutor>>();

        private const int PIPELINES_COUNT = 8;
        private const int VARIATIONS_COUNT = 256;
        private const int VARIATION_MAX_INDEX = VARIATIONS_COUNT - 1;
        private static readonly IReadOnlyList<bool>[] _enabledVariations = new Variations<bool>(new[] { true, false }, PIPELINES_COUNT, GenerateOption.WithRepetition).ToArray();

        [SetUp]
        public void SetUp()
        {
            _pipelineExecutorFactory = Substitute.For<Func<IPipelineExecutor>>();
        }

        [Test]
        [Combinatorial]
        public void EnabledPipelinesExecuted([Range(0, VARIATION_MAX_INDEX)] int variationIndex)
        {
            // Arrange
            var fixture = new Fixture();
            IReadOnlyList<bool> variation = _enabledVariations[variationIndex];
            int enabledCount = variation.Count(e => e);
            var configuration = new SimpleBackupConfiguration
            {
                BackupPipelines = Enumerable.Range(0, PIPELINES_COUNT).Select(i =>

                    new BackupPipeline
                    {
                        Name = fixture.Create<string>(),
                        Enabled = variation[i]
                    }
                ).ToArray()
            };

            var executor = Substitute.For<IPipelineExecutor>();
            _pipelineExecutorFactory.Invoke().Returns(executor);

            var engine = new SimpleBackup.Engine.Engine(_logger, configuration, _pipelineExecutorFactory);

            // Act
            engine.Execute();

            // Assert
            _pipelineExecutorFactory.Received(enabledCount).Invoke();
            executor.Received(enabledCount).Execute(Arg.Is<BackupPipeline>(p => configuration.BackupPipelines.Single(c => c.Name.Equals(p.Name)).Enabled));
        }
    }
}
