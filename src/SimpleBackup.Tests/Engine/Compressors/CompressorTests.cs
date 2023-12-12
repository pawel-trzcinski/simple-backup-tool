using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Configuration;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors
{
    [TestFixture]
    public class CompressorTests
    {
        private class CompressorTester(IFileSystemService fileSystemService, IZipWrapper zipWrapper, IThresholdGuard thresholdGuard, IArchiveDiskManager archiveDiskManager)
            : Compressor(Substitute.For<ILogger>(), fileSystemService, zipWrapper, thresholdGuard, archiveDiskManager)
        {
            private readonly List<FileSystemEntity> _entities = new List<FileSystemEntity>();
            private readonly List<string> _files = new List<string>();
            private readonly List<CompressionType> _types = new List<CompressionType>();

            protected override void CompressDirectory(FileSystemEntity fileSystemEntity, string zipFile, CompressionType compressionType)
            {
                _entities.Add(fileSystemEntity);
                _files.Add(zipFile);
                _types.Add(compressionType);
            }

            public void AssertDirectoryCompression(string[] sources, string[] zipFiles, CompressionType compressionType)
            {
                Assert.That(sources, Has.Length.EqualTo(zipFiles.Length));
                Assert.That(_types.All(t => t == compressionType), Is.True);

                for (int i = 0; i < sources.Length; i++)
                {
                    _ = _entities.Single(e => e.Source.Equals(sources[i]));
                    _ = _files.Single(f => f.Equals(zipFiles[i]));
                }

                Assert.That(_entities.Any(e => e.Type == FileSystemEntityType.File), Is.False);
            }

            public void AssertEmptyDirectoryCompression()
            {
                Assert.That(_entities, Has.Count.Zero);
                Assert.That(_files, Has.Count.Zero);
                Assert.That(_types, Has.Count.Zero);
            }
        }

        private IFileSystemService _fileSystemService = Substitute.For<IFileSystemService>();
        private IZipWrapper _zipWrapper = Substitute.For<IZipWrapper>();
        private IThresholdGuard _thresholdGuard = Substitute.For<IThresholdGuard>();
        private IArchiveDiskManager _archiveDiskManager = Substitute.For<IArchiveDiskManager>();

        [SetUp]
        public void SetUp()
        {
            _fileSystemService = Substitute.For<IFileSystemService>();
            _zipWrapper = Substitute.For<IZipWrapper>();
            _thresholdGuard = Substitute.For<IThresholdGuard>();
            _archiveDiskManager = Substitute.For<IArchiveDiskManager>();
        }

        [Test]
        [Combinatorial]
        public void FilesAndFoldersCompressedWithKnownType(
            [Values] bool enabled,
            [Values] bool removeOldArchive,
            [Values] CompressionType compressionType)
        {
            // Arrange
            var fixture = new Fixture();
            string mainFolder = fixture.Create<string>();
            string archiveFolder = fixture.Create<string>();
            string finishedArchiveFolder = fixture.Create<string>();
            string source1 = fixture.Create<string>() + ".mp3";
            string source2 = fixture.Create<string>();
            string source3 = fixture.Create<string>() + ".avi";
            string source4 = fixture.Create<string>();
            string source5 = fixture.Create<string>() + ".txt";
            string zipFile1 = fixture.Create<string>();
            string zipFile2 = fixture.Create<string>();
            string zipFile3 = fixture.Create<string>();
            string zipFile4 = fixture.Create<string>();
            string zipFile5 = fixture.Create<string>();

            var backupPipeline = new BackupPipeline
            {
                BackupOutputFolder = fixture.Create<string>(),
                Compression = compressionType,
                Enabled = enabled,
                Name = fixture.Create<string>(),
                RefreshThreshold = TimeSpan.FromSeconds(50),
                RemoveOldArchive = removeOldArchive,
                Sources = new string[]
                {
                    source1,
                    source2,
                    source3,
                    source4,
                    source5
                }
            };

            _archiveDiskManager.PrepareMainFolder(backupPipeline.BackupOutputFolder, backupPipeline.Name).Returns(mainFolder);
            _archiveDiskManager.PrepareArchiveFolder(mainFolder).Returns(archiveFolder);

            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source1)), Arg.Any<int>()).Returns(zipFile1);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source2)), Arg.Any<int>()).Returns(zipFile2);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source3)), Arg.Any<int>()).Returns(zipFile3);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source4)), Arg.Any<int>()).Returns(zipFile4);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source5)), Arg.Any<int>()).Returns(zipFile5);

            _archiveDiskManager.FinishArchive(archiveFolder).Returns(finishedArchiveFolder);

            _thresholdGuard.RefreshThresholdPassed(mainFolder, backupPipeline.RefreshThreshold).Returns(true);

            _fileSystemService.RecognizeEntity(source1).Returns(new FileSystemEntity(FileSystemEntityType.File, source1));
            _fileSystemService.RecognizeEntity(source2).Returns(new FileSystemEntity(FileSystemEntityType.Direcotry, source2));
            _fileSystemService.RecognizeEntity(source3).Returns(new FileSystemEntity(FileSystemEntityType.File, source3));
            _fileSystemService.RecognizeEntity(source4).Returns(new FileSystemEntity(FileSystemEntityType.Direcotry, source4));
            _fileSystemService.RecognizeEntity(source5).Returns(new FileSystemEntity(FileSystemEntityType.File, source5));

            var compressor = new CompressorTester(_fileSystemService, _zipWrapper, _thresholdGuard, _archiveDiskManager);

            // Act
            compressor.Compress(backupPipeline);

            // Assert
            _zipWrapper.Received(1).CompressFile(zipFile1, source1, CompressionLevelDiscoverer.Get(source1, compressionType));
            _zipWrapper.Received(1).CompressFile(zipFile3, source3, CompressionLevelDiscoverer.Get(source3, compressionType));
            _zipWrapper.Received(1).CompressFile(zipFile5, source5, CompressionLevelDiscoverer.Get(source5, compressionType));

            _fileSystemService.Received(removeOldArchive ? 1 : 0).RemoveAllDirectoriesExcept(mainFolder, finishedArchiveFolder);

            compressor.AssertDirectoryCompression(new[] { source2, source4 }, new[] { zipFile2, zipFile4 }, compressionType);
        }

        [Test]
        public void ThresholdGuardForbidsCompression(
            [Values] bool enabled,
            [Values] bool removeOldArchive,
            [Values(CompressionType.Minimal, CompressionType.Normal, CompressionType.Best)]
            CompressionType compressionType
        )
        {
            // Arrange
            var fixture = new Fixture();
            string mainFolder = fixture.Create<string>();
            string archiveFolder = fixture.Create<string>();
            string finishedArchiveFolder = fixture.Create<string>();
            string source1 = fixture.Create<string>();
            string source2 = fixture.Create<string>();
            string source3 = fixture.Create<string>();
            string source4 = fixture.Create<string>();
            string source5 = fixture.Create<string>();
            string zipFile1 = fixture.Create<string>();
            string zipFile2 = fixture.Create<string>();
            string zipFile3 = fixture.Create<string>();
            string zipFile4 = fixture.Create<string>();
            string zipFile5 = fixture.Create<string>();

            var backupPipeline = new BackupPipeline()
            {
                BackupOutputFolder = fixture.Create<string>(),
                Compression = compressionType,
                Enabled = enabled,
                Name = fixture.Create<string>(),
                RefreshThreshold = TimeSpan.FromSeconds(50),
                RemoveOldArchive = removeOldArchive,
                Sources = new string[]
                {
                    source1,
                    source2,
                    source3,
                    source4,
                    source5
                }
            };

            _archiveDiskManager.PrepareMainFolder(backupPipeline.BackupOutputFolder, backupPipeline.Name).Returns(mainFolder);
            _archiveDiskManager.PrepareArchiveFolder(mainFolder).Returns(archiveFolder);

            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source1)), Arg.Any<int>()).Returns(zipFile1);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source2)), Arg.Any<int>()).Returns(zipFile2);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source3)), Arg.Any<int>()).Returns(zipFile3);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source4)), Arg.Any<int>()).Returns(zipFile4);
            _archiveDiskManager.PrepareEntryFolder(archiveFolder, Arg.Is<FileSystemEntity>(e => e.Source.Equals(source5)), Arg.Any<int>()).Returns(zipFile5);

            _archiveDiskManager.FinishArchive(archiveFolder).Returns(finishedArchiveFolder);

            _thresholdGuard.RefreshThresholdPassed(mainFolder, backupPipeline.RefreshThreshold).Returns(false);

            _fileSystemService.RecognizeEntity(source1).Returns(new FileSystemEntity(FileSystemEntityType.File, source1));
            _fileSystemService.RecognizeEntity(source2).Returns(new FileSystemEntity(FileSystemEntityType.Direcotry, source2));
            _fileSystemService.RecognizeEntity(source3).Returns(new FileSystemEntity(FileSystemEntityType.File, source3));
            _fileSystemService.RecognizeEntity(source4).Returns(new FileSystemEntity(FileSystemEntityType.Direcotry, source4));
            _fileSystemService.RecognizeEntity(source5).Returns(new FileSystemEntity(FileSystemEntityType.File, source5));

            var compressor = new CompressorTester(_fileSystemService, _zipWrapper, _thresholdGuard, _archiveDiskManager);

            // Act
            compressor.Compress(backupPipeline);

            // Assert
            _zipWrapper.Received(0).CompressFile(zipFile1, source1, CompressionLevelDiscoverer.Get(source1, compressionType));
            _zipWrapper.Received(0).CompressFile(zipFile3, source3, CompressionLevelDiscoverer.Get(source3, compressionType));
            _zipWrapper.Received(0).CompressFile(zipFile5, source5, CompressionLevelDiscoverer.Get(source5, compressionType));

            _fileSystemService.Received(0).RemoveAllDirectoriesExcept(mainFolder, finishedArchiveFolder);

            compressor.AssertEmptyDirectoryCompression();
        }
    }
}
