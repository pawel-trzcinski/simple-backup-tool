using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using SimpleBackup.Configuration;
using SimpleBackup.Engine;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine;

[TestFixture]
public class PipelineExecutorTests
{
    private ICompressorFactory _compressorFactory = Substitute.For<ICompressorFactory>();
    private ICompressor _compressor = Substitute.For<ICompressor>();

    [SetUp]
    public void SetUp()
    {
        _compressorFactory = Substitute.For<ICompressorFactory>();
        _compressor = Substitute.For<ICompressor>();
        _compressorFactory.Create(Arg.Any<CompressionType>()).Returns(_compressor);
    }

    [Test]
    public void ExecutionPassesArguments()
    {
        // Arrange
        var fixture = new Fixture();
        var pipeline = fixture.Create<BackupPipeline>();
        var executor = new PipelineExecutor(_compressorFactory);

        // Act
        executor.Execute(pipeline);

        // Assert
        _compressor.Received(1).Compress(Arg.Is<BackupPipeline>(p => p.Name.Equals(pipeline.Name)));
    }
}
