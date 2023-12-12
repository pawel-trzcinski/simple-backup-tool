using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors
{
    [TestFixture]
    public class ArchiveDiskManagerTests
    {
        private static readonly ILogger _logger = Substitute.For<ILogger>();
        private IFileSystemService _fileSystemService = Substitute.For<IFileSystemService>();
        private IArchiveNameService _archiveNameService = Substitute.For<IArchiveNameService>();

        [SetUp]
        public void SetUp()
        {
            _fileSystemService = Substitute.For<IFileSystemService>();
            _archiveNameService = Substitute.For<IArchiveNameService>();
        }

        [Test]
        public void PrepareMainFolder([Values] bool folderExists)
        {
            // Arrange
            var fixture = new Fixture();
            string backupOutputFolder = fixture.Create<string>();
            string pipelineName = fixture.Create<string>();
            string mainFolderExpected = Path.Combine(backupOutputFolder, pipelineName);

            _fileSystemService.DirectoryExists(mainFolderExpected).Returns(folderExists);

            var manager = new ArchiveDiskManager(_logger, _fileSystemService, _archiveNameService);

            // Act
            string mainFolder = manager.PrepareMainFolder(backupOutputFolder, pipelineName);

            // Assert
            Assert.That(mainFolder, Is.EqualTo(mainFolderExpected));
            _fileSystemService.Received(folderExists ? 0 : 1).CreateDirectory(mainFolder);
        }

        [Test]
        public void PrepareArchiveFolder()
        {
            // Arrange
            var fixture = new Fixture();
            string mainFolder = fixture.Create<string>();
            string archiveFolderName = fixture.Create<string>();

            string archiveFolderExpected = Path.Combine(mainFolder, archiveFolderName);

            _archiveNameService.ConstructArchiveFolderName().Returns(archiveFolderName);

            _fileSystemService.DirectoryExists(archiveFolderExpected).Returns(false);

            var manager = new ArchiveDiskManager(_logger, _fileSystemService, _archiveNameService);

            // Act
            string archiveFolder = manager.PrepareArchiveFolder(mainFolder);

            // Assert
            Assert.That(archiveFolder, Is.EqualTo(archiveFolderExpected));
            _fileSystemService.Received(1).CreateDirectory(archiveFolder);
        }

        [Test]
        public void ArchiveFolderAlreadyExists()
        {
            // Arrange
            var fixture = new Fixture();
            string mainFolder = fixture.Create<string>();
            string archiveFolderName = fixture.Create<string>();

            string archiveFolderExpected = Path.Combine(mainFolder, archiveFolderName);

            _archiveNameService.ConstructArchiveFolderName().Returns(archiveFolderName);

            _fileSystemService.DirectoryExists(archiveFolderExpected).Returns(true);

            var manager = new ArchiveDiskManager(_logger, _fileSystemService, _archiveNameService);

            // Act
            var invalidOperationException = Assert.Throws<InvalidOperationException>(() => manager.PrepareArchiveFolder(mainFolder));

            // Assert
            Assert.That(invalidOperationException, Is.Not.Null);
            _fileSystemService.Received(0).CreateDirectory(Arg.Any<string>());
        }

        [Test]
        public void PrepareEntryFolder()
        {
            // Arrange
            var fixture = new Fixture();
            string archiveFolder = fixture.Create<string>();
            string sourceName = fixture.Create<string>();
            var fileSystemEntity = new FileSystemEntity
            (
                fixture.Create<FileSystemEntityType>(),
                Path.Combine(TestContext.CurrentContext.WorkDirectory, sourceName)
            );

            int index = Math.Abs(fixture.Create<int>()) % 1000;
            string entryFolder = Path.Combine(archiveFolder, $"{ArchiveDiskManager.ENTRY}{index:000}");
            string zipFileExpected = Path.Combine(entryFolder, $"{sourceName}.zip");

            var manager = new ArchiveDiskManager(_logger, _fileSystemService, _archiveNameService);

            // Act
            string zipFile = manager.PrepareEntryFolder(archiveFolder, fileSystemEntity, index);

            // Assert
            Assert.That(zipFile, Is.EqualTo(zipFileExpected));
            _fileSystemService.Received(1).CreateDirectory(entryFolder);
            _fileSystemService.Received(1).WriteTextToFile(Path.Combine(entryFolder, $"{sourceName}.{ArchiveDiskManager.SOURCE}"), fileSystemEntity.Source);
        }

        [Test]
        public void FinishArchive()
        {
            // Arrange
            var fixture = new Fixture();
            string archiveFolder = fixture.Create<string>();
            string newArchiveFolderExpected = $"{archiveFolder}.{IArchiveNameService.FINISHED}";

            var manager = new ArchiveDiskManager(_logger, _fileSystemService, _archiveNameService);

            // Act
            string newArchiveFolder = manager.FinishArchive(archiveFolder);

            // Assert
            Assert.That(newArchiveFolder, Is.EqualTo(newArchiveFolderExpected));
            _fileSystemService.Received(1).MoveDirectory(archiveFolder, newArchiveFolder);
        }
    }
}
