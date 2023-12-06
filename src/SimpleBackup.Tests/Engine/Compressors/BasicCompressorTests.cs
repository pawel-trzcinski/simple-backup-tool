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
        : BasicCompressor(Substitute.For<ILogger>(), Substitute.For<IFileSystemService>(), zipWrapper, Substitute.For<IArchiveNameService>())
    {
        public void CompressDirectoryExposed(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType, bool testRun)
        {
            CompressDirectory(fileSystemEntity, zipFile, compressionType, testRun);
        }
    }

    [Test]
    public void CompressionTypeMustNotBeAdaptive([Values] bool testRun)
    {
        // Arrange
        var fixture = new Fixture();
        var fileSystemEntity = fixture.Create<FileSystemEntity>();
        var zipFile = fixture.Create<string>();

        var zipWrapper = Substitute.For<IZipWrapper>();

        var compressor = new BasicCompressorTester(zipWrapper);

        // Act, Assert
        Assert.Throws<NotSupportedException>(() => compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, CompressionType.Adaptive, testRun));
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
        compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, compressionType, false);

        // Assert
        zipWrapper.Received(1).CompressDirectory(zipFile, fileSystemEntity.Source, compressionLevel);
    }

    [TestCase(CompressionType.Best)]
    [TestCase(CompressionType.Minimal)]
    [TestCase(CompressionType.Normal)]
    public void TestRunGuardWorks(CompressionType compressionType)
    {
        // Arrange
        var fixture = new Fixture();
        var fileSystemEntity = fixture.Create<FileSystemEntity>();
        var zipFile = fixture.Create<string>();

        var zipWrapper = Substitute.For<IZipWrapper>();

        var compressor = new BasicCompressorTester(zipWrapper);

        // Act
        compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, compressionType, true);

        // Assert
        zipWrapper.DidNotReceiveWithAnyArgs();
    }
}
