using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;
using static SimpleBackup.Abstractions.IZipWrapper;

namespace SimpleBackup.Tests.Engine.Compressors
{
    [TestFixture]
    public class AdaptiveCompressorTests
    {
        private sealed class AdaptiveCompressorTester(IZipWrapper zipWrapper)
            : AdaptiveCompressor(Substitute.For<ILogger>(), Substitute.For<IFileSystemService>(), zipWrapper, Substitute.For<IArchiveNameService>())
        {
            public void CompressDirectoryExposed(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
            {
                CompressDirectory(fileSystemEntity, zipFile, compressionType);
            }
        }

        [Test]
        [Combinatorial]
        public void CompressionTypeMustBeAdaptive([Values(CompressionType.Best, CompressionType.Minimal, CompressionType.Normal)] CompressionType compressionType)
        {
            // Arrange
            var fixture = new Fixture();
            var fileSystemEntity = fixture.Create<FileSystemEntity>();
            var zipFile = fixture.Create<string>();

            var zipWrapper = Substitute.For<IZipWrapper>();

            var compressor = new AdaptiveCompressorTester(zipWrapper);

            // Act, Assert
            Assert.Throws<NotSupportedException>(() => compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, compressionType));
        }

        [Test]
        public void CompressionZips()
        {
            // Arrange
            var fixture = new Fixture();
            var fileSystemEntity = fixture.Create<FileSystemEntity>();
            var zipFile = fixture.Create<string>();

            var zipWrapper = Substitute.For<IZipWrapper>();
            // zipWrapper.

            var compressor = new AdaptiveCompressorTester(zipWrapper);

            // Act
            compressor.CompressDirectoryExposed(fileSystemEntity, zipFile, CompressionType.Adaptive);

            // Assert
            zipWrapper.Received(1).CompressDirectory(zipFile, fileSystemEntity.Source, Arg.Any<CompressionLevelFunc>());
        }
    }
}
