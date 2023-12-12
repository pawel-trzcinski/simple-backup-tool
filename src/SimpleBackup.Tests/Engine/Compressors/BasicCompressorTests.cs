using System.IO.Compression;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors;

[TestFixture]
public class BasicCompressorTests
{
    private sealed class BasicCompressorTester(IZipWrapper zipWrapper)
        : BasicCompressor(Substitute.For<ILogger>(), Substitute.For<IFileSystemService>(), zipWrapper, Substitute.For<IThresholdGuard>(), Substitute.For<IArchiveDiskManager>())
    {
        public void CompressDirectoryExposed(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
        {
            CompressDirectory(fileSystemEntity, zipFile, compressionType);
        }
    }

    [Test]
    public void CompressionTypeMustNotBeAdaptive()
    {
        // Arrange
        var fixture = new Fixture();
        var fileSystemEntity = fixture.Create<FileSystemEntity>();
        var zipFile = fixture.Create<string>();

        var zipWrapper = Substitute.For<IZipWrapper>();

        var compressor = new BasicCompressorTester(zipWrapper);

        // Act, Assert
        Assert.Throws<NotSupportedException>(() => compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, CompressionType.Adaptive));
    }

    [TestCase(CompressionType.Best, CompressionLevel.SmallestSize)]
    [TestCase(CompressionType.Minimal, CompressionLevel.Fastest)]
    [TestCase(CompressionType.Normal, CompressionLevel.Optimal)]
    public void CompressionZips(CompressionType compressionType, CompressionLevel compressionLevel)
    {
        // Arrange
        var fixture = new Fixture();
        var fileSystemEntity = fixture.Create<FileSystemEntity>();
        var zipFile = fixture.Create<string>();

        var zipWrapper = Substitute.For<IZipWrapper>();

        var compressor = new BasicCompressorTester(zipWrapper);

        // Act
        compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, compressionType);

        // Assert
        zipWrapper.Received(1).CompressDirectory(zipFile, fileSystemEntity.Source, compressionLevel);
    }
}
