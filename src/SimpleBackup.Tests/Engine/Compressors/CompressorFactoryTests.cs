using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors;

[TestFixture]
public class CompressorFactoryTests
{
    private Func<IBasicCompressor> _basicCompressorFactory = Substitute.For<Func<IBasicCompressor>>();
    private Func<IAdaptiveCompressor> _adaptiveCompressorFactory = Substitute.For<Func<IAdaptiveCompressor>>();

    [SetUp]
    public void SetUp()
    {
        _basicCompressorFactory = Substitute.For<Func<IBasicCompressor>>();
        _basicCompressorFactory.Invoke().Returns(Substitute.For<IBasicCompressor>());

        _adaptiveCompressorFactory = Substitute.For<Func<IAdaptiveCompressor>>();
        _adaptiveCompressorFactory.Invoke().Returns(Substitute.For<IAdaptiveCompressor>());
    }

    [TestCase(CompressionType.Minimal)]
    [TestCase(CompressionType.Normal)]
    [TestCase(CompressionType.Best)]
    public void BasicCompressorFactorySelected(CompressionType compressionType)
    {
        // Arrange
        var factory = new CompressorFactory(Substitute.For<ILogger>(), _basicCompressorFactory, _adaptiveCompressorFactory);

        // Act
        ICompressor compressor = factory.Create(compressionType);

        // Assert
        Assert.That(compressor, Is.AssignableTo<IBasicCompressor>());
    }

    [TestCase(CompressionType.Adaptive)]
    public void AdvancedCompressorFactorySelected(CompressionType compressionType)
    {
        // Arrange
        var factory = new CompressorFactory(Substitute.For<ILogger>(), _basicCompressorFactory, _adaptiveCompressorFactory);

        // Act
        ICompressor compressor = factory.Create(compressionType);

        // Assert
        Assert.That(compressor, Is.AssignableTo<IAdaptiveCompressor>());
    }
}

