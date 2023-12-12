using NUnit.Framework;
using System.IO.Compression;
using AutoFixture;
using NSubstitute;
using Serilog;
using SimpleBackup.Abstractions;

namespace SimpleBackup.Tests.Abstractions
{
    [TestFixture]
    public class ZipWrapperTests
    {
        private static readonly ILogger _logger = Substitute.For<ILogger>();

        private static readonly string _baseFolder = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString());

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(_baseFolder))
            {
                TearDown();
            }

            Directory.CreateDirectory(_baseFolder);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_baseFolder, true);
        }

        [Test]
        public void CompressedFileExists([Values] CompressionLevel compressionLevel)
        {
            // Arrange
            var fixture = new Fixture();
            string sourceFile = Path.Combine(_baseFolder, fixture.Create<string>());
            string sourceFileContent = fixture.Create<string>();
            string destinationFile = Path.Combine(_baseFolder, $"{fixture.Create<string>()}.zip");

            File.WriteAllText(sourceFile, sourceFileContent);

            var zipWrapper = new ZipWrapper(_logger);

            // Act
            zipWrapper.CompressFile(destinationFile, sourceFile, compressionLevel);

            // Assert
            Assert.That(File.Exists(destinationFile));
            using (var archive = ZipFile.Open(destinationFile, ZipArchiveMode.Read))
            {
                Assert.That(archive.Entries, Has.Count.EqualTo(1));

                ZipArchiveEntry entry = archive.Entries.Single();
                Assert.That(entry.Name, Is.EqualTo(Path.GetFileName(sourceFile)));
                using (StreamReader reader = new StreamReader(entry.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent));
                }
            }
        }

        [Test]
        public void SimpleCompressedFolderExists([Values] CompressionLevel compressionLevel)
        {
            // Arrange
            var fixture = new Fixture();
            string sourceDirectory = Path.Combine(_baseFolder, fixture.Create<string>());
            string sourceFile1 = Path.Combine(sourceDirectory, fixture.Create<string>());
            string sourceFileContent1 = fixture.Create<string>();
            string sourceFile2 = Path.Combine(sourceDirectory, fixture.Create<string>());
            string sourceFileContent2 = fixture.Create<string>();
            string sourceFile3 = Path.Combine(sourceDirectory, fixture.Create<string>(), fixture.Create<string>());
            string sourceFileContent3 = fixture.Create<string>();
            string destinationFile = Path.Combine(_baseFolder, $"{fixture.Create<string>()}.zip");

            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(sourceFile1, sourceFileContent1);
            File.WriteAllText(sourceFile2, sourceFileContent2);
            Directory.CreateDirectory(Path.GetDirectoryName(sourceFile3)!);
            File.WriteAllText(sourceFile3, sourceFileContent3);

            var zipWrapper = new ZipWrapper(_logger);

            // Act
            zipWrapper.CompressDirectory(destinationFile, sourceDirectory, compressionLevel);

            // Assert
            Assert.That(File.Exists(destinationFile));
            using (var archive = ZipFile.Open(destinationFile, ZipArchiveMode.Read))
            {
                Assert.That(archive.Entries, Has.Count.EqualTo(3));

                ZipArchiveEntry entry1 = archive.Entries.Single(e => e.FullName.Equals(sourceFile1.Substring(_baseFolder.Length + 1).Replace('\\', '/')));
                ZipArchiveEntry entry2 = archive.Entries.Single(e => e.FullName.Equals(sourceFile2.Substring(_baseFolder.Length + 1).Replace('\\', '/')));
                ZipArchiveEntry entry3 = archive.Entries.Single(e => e.FullName.Equals(sourceFile3.Substring(_baseFolder.Length + 1).Replace('\\', '/')));

                using (StreamReader reader = new StreamReader(entry1.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent1));
                }

                using (StreamReader reader = new StreamReader(entry2.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent2));
                }

                using (StreamReader reader = new StreamReader(entry3.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent3));
                }
            }
        }

        [Test]
        public void AdaptiveCompressedFolderExists([Values] CompressionLevel compressionLevel)
        {
            // Arrange
            var fixture = new Fixture();
            string sourceDirectory = Path.Combine(_baseFolder, fixture.Create<string>());
            string sourceFile1 = Path.Combine(sourceDirectory, fixture.Create<string>());
            string sourceFileContent1 = fixture.Create<string>();
            string sourceFile2 = Path.Combine(sourceDirectory, fixture.Create<string>());
            string sourceFileContent2 = fixture.Create<string>();
            string sourceFile3 = Path.Combine(sourceDirectory, fixture.Create<string>(), fixture.Create<string>());
            string sourceFileContent3 = fixture.Create<string>();
            string destinationFile = Path.Combine(_baseFolder, $"{fixture.Create<string>()}.zip");

            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(sourceFile1, sourceFileContent1);
            File.WriteAllText(sourceFile2, sourceFileContent2);
            Directory.CreateDirectory(Path.GetDirectoryName(sourceFile3)!);
            File.WriteAllText(sourceFile3, sourceFileContent3);

            var zipWrapper = new ZipWrapper(_logger);

            // Act
            zipWrapper.CompressDirectory(destinationFile, sourceDirectory, _ => compressionLevel);

            // Assert
            Assert.That(File.Exists(destinationFile));
            using (var archive = ZipFile.Open(destinationFile, ZipArchiveMode.Read))
            {
                Assert.That(archive.Entries, Has.Count.EqualTo(3));

                ZipArchiveEntry entry1 = archive.Entries.Single(e => e.FullName.Equals(sourceFile1.Substring(_baseFolder.Length + 1).Replace('\\', '/')));
                ZipArchiveEntry entry2 = archive.Entries.Single(e => e.FullName.Equals(sourceFile2.Substring(_baseFolder.Length + 1).Replace('\\', '/')));
                ZipArchiveEntry entry3 = archive.Entries.Single(e => e.FullName.Equals(sourceFile3.Substring(_baseFolder.Length + 1).Replace('\\', '/')));

                using (StreamReader reader = new StreamReader(entry1.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent1));
                }

                using (StreamReader reader = new StreamReader(entry2.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent2));
                }

                using (StreamReader reader = new StreamReader(entry3.Open()))
                {
                    string actualtContent = reader.ReadToEnd();
                    Assert.That(actualtContent, Is.EqualTo(sourceFileContent3));
                }
            }
        }
    }
}
