using System.IO.Compression;
using AutoFixture;
using NUnit.Framework;
using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors;

[TestFixture]
public class CompressionLevelDiscovererTests
{
    private static readonly string[] _minimalExtensions = CompressionLevelDiscoverer.MinimalExtensions.ToArray();
    private static readonly string[] _bestExtensions = CompressionLevelDiscoverer.BestExtensions.ToArray();

    [TestCase(CompressionType.Best, CompressionLevel.SmallestSize)]
    [TestCase(CompressionType.Minimal, CompressionLevel.Fastest)]
    [TestCase(CompressionType.Normal, CompressionLevel.Optimal)]
    public void NonAdaptive(CompressionType compressionType, CompressionLevel compressionLevel)
    {
        // Arrange
        var fixture = new Fixture();
        string filename = $"{fixture.Create<string>()}.{fixture.Create<string>()}";

        // Act
        CompressionLevel level = CompressionLevelDiscoverer.Get(filename, compressionType);

        // Assert
        Assert.That(level, Is.EqualTo(compressionLevel));
    }

    [Test]
    public void DiscoverMinimal([ValueSource(nameof(_minimalExtensions))] string extension)
    {
        // Arrange
        string filename = $"fileName{extension}";

        // Act
        CompressionLevel level = CompressionLevelDiscoverer.Get(filename, CompressionType.Adaptive);

        // Assert
        Assert.That(level, Is.EqualTo(CompressionLevel.Fastest));
    }

    [Test]
    public void DiscoverBest([ValueSource(nameof(_bestExtensions))] string extension)
    {
        // Arrange
        string filename = $"fileName{extension}";

        // Act
        CompressionLevel level = CompressionLevelDiscoverer.Get(filename, CompressionType.Adaptive);

        // Assert
        Assert.That(level, Is.EqualTo(CompressionLevel.SmallestSize));
    }

    [Test]
    public void DiscoverNormel()
    {
        // Arrange
        var fixture = new Fixture();
        string filename = $"fileName.{fixture.Create<string>()}";

        // Act
        CompressionLevel level = CompressionLevelDiscoverer.Get(filename, CompressionType.Adaptive);

        // Assert
        Assert.That(level, Is.EqualTo(CompressionLevel.Optimal));
    }
}
