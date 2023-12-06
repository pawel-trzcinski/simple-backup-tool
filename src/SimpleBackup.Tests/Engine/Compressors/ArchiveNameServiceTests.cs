using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using SimpleBackup.Abstractions;
using SimpleBackup.Engine.Compressors;
using ILogger = Serilog.ILogger;

namespace SimpleBackup.Tests.Engine.Compressors
{
    [TestFixture]
    public class ArchiveNameServiceTests
    {
        private IDateTimeService _dateTimeService = Substitute.For<IDateTimeService>();
        private IFileSystemService _fileSystemService = Substitute.For<IFileSystemService>();

        [SetUp]
        public void SetUp()
        {
            _dateTimeService = Substitute.For<IDateTimeService>();
            _fileSystemService = Substitute.For<IFileSystemService>();
        }

        [Test]
        public void ConstructArchiveFolderNameUsesDateTimeService()
        {
            // Arrange
            var fixture = new Fixture();
            var now = fixture.Create<DateTime>();

            _dateTimeService.Now.Returns(now);

            var service = new ArchiveNameService(_dateTimeService, _fileSystemService);

            // Act
            string folderName = service.ConstructArchiveFolderName();

            // Arrange
            Assert.That(folderName, Is.EqualTo($"{ArchiveNameService.ARCHIVE}_{now.ToString(ArchiveNameService.DATE_TIME_PATTERN)}"));
            Assert.That(folderName, Is.EqualTo($"{ArchiveNameService.ARCHIVE}_{now.ToString(ArchiveNameService.DATE_TIME_PATTERN)}"));
        }

        [Test]
        public void ReadingArchiveFoldersTimes()
        {
            // Arrange
            var fixture = new Fixture();

            var now = DateTime.Now;
            var now1 = now + TimeSpan.FromSeconds(1);
            var now2 = now + TimeSpan.FromSeconds(2);
            var now3 = now + TimeSpan.FromSeconds(3);
            var now4 = now + TimeSpan.FromSeconds(4);
            var now5 = now + TimeSpan.FromSeconds(5);

            _dateTimeService.Now.Returns(now1, now2, now3, now4, now5, now);

            var fileSystemService = new FileSystemService(Substitute.For<ILogger>());
            var mainFolderName = fixture.Create<string>();
            var mainFolder = Path.Combine(TestContext.CurrentContext.WorkDirectory, mainFolderName);

            try
            {
                var service = new ArchiveNameService(_dateTimeService, new FileSystemService(Substitute.For<ILogger>()));

                fileSystemService.CreateDirectory(mainFolder);
                fileSystemService.CreateDirectory(Path.Combine(mainFolder, service.ConstructArchiveFolderName()));
                fileSystemService.CreateDirectory(Path.Combine(mainFolder, service.ConstructArchiveFolderName()));
                fileSystemService.CreateDirectory(Path.Combine(mainFolder, $"{service.ConstructArchiveFolderName()}.{IArchiveNameService.FINISHED}"));
                fileSystemService.CreateDirectory(Path.Combine(mainFolder, service.ConstructArchiveFolderName()));
                fileSystemService.CreateDirectory(Path.Combine(mainFolder, service.ConstructArchiveFolderName()));

                // Act
                TimeSpan timeSpan = service.GetTimePassedFromLatesFinishedArchive(mainFolder);

                // Arrange
                Assert.That((now3 - now).TotalSeconds, Is.EqualTo(3));
            }
            finally
            {
                if (Directory.Exists(mainFolder))
                {
                    Directory.Delete(mainFolder, true);
                }
            }
        }
    }
}
