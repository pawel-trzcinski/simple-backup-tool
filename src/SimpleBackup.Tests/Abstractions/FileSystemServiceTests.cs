using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using SimpleBackup.Abstractions;

namespace SimpleBackup.Tests.Abstractions;

[TestFixture]
public class FileSystemServiceTests
{
    private static readonly string _testFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "file.txt");
    private static readonly string _testFile2 = Path.Combine(TestContext.CurrentContext.WorkDirectory, "file.txt");
    private static readonly string _testDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "files");
    private static readonly string _testDirectory2 = Path.Combine(TestContext.CurrentContext.WorkDirectory, "files2");
    private static readonly string _innerTestDirectory1 = Path.Combine(_testDirectory, "what1");
    private static readonly string _innerTestDirectory2 = Path.Combine(_testDirectory, "what2");
    private static readonly string _innerTestDirectory3 = Path.Combine(_testDirectory, "what3");

    private static readonly string _testDirectoryA = Path.Combine(_testDirectory, "files11_abc");
    private static readonly string _testDirectoryB = Path.Combine(_testDirectory, "files11_bcd");
    private static readonly string _testDirectoryC = Path.Combine(_testDirectory, "files22_cde");
    private static readonly string _testDirectoryX = Path.Combine(_testDirectoryA, "files11_abc");
    private static readonly string _testDirectoryY = Path.Combine(_testDirectoryA, "files11_bcd");
    private static readonly string _testDirectoryZ = Path.Combine(_testDirectoryB, "files22_cde");

    private FileSystemService _service = new FileSystemService(Substitute.For<Serilog.ILogger>());

    [SetUp]
    public void SetUp()
    {
        if (!File.Exists(_testFile))
        {
            File.WriteAllText(_testFile, "test");
        }

        if (!Directory.Exists(_testDirectory))
        {
            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(_innerTestDirectory1);
            Directory.CreateDirectory(_innerTestDirectory2);
            Directory.CreateDirectory(_innerTestDirectory3);

            Directory.CreateDirectory(_testDirectoryA);
            Directory.CreateDirectory(_testDirectoryB);
            Directory.CreateDirectory(_testDirectoryC);
            Directory.CreateDirectory(_testDirectoryX);
            Directory.CreateDirectory(_testDirectoryY);
            Directory.CreateDirectory(_testDirectoryZ);
        }

        if (Directory.Exists(_testDirectory2))
        {
            Directory.Delete(_testDirectory2);
        }

        _service = new FileSystemService(Substitute.For<Serilog.ILogger>());
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFile))
        {
            File.Delete(_testFile);
        }

        if (File.Exists(_testFile2))
        {
            File.Delete(_testFile2);
        }

        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        if (Directory.Exists(_testDirectory2))
        {
            Directory.Delete(_testDirectory2);
        }
    }

    [Test]
    public void RecognizeEntityFile()
    {
        // Act
        FileSystemEntity entity = _service.RecognizeEntity(_testFile);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(entity.Type, Is.EqualTo(FileSystemEntityType.File));
            Assert.That(entity.Source, Is.EqualTo(_testFile));
        });
    }

    [Test]
    public void RecognizeEntityDirectory()
    {
        // Act
        FileSystemEntity entity = _service.RecognizeEntity(_testDirectory);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(entity.Type, Is.EqualTo(FileSystemEntityType.Direcotry));
            Assert.That(entity.Source, Is.EqualTo(_testDirectory));
        });
    }

    [Test]
    public void DirectoryExists([Values] bool changeDirectoryName)
    {
        // Arrange
        string directoryToCheck = _testDirectory + (changeDirectoryName ? "XYZ" : String.Empty);

        // Act
        bool exists = _service.DirectoryExists(directoryToCheck);

        // Assert
        Assert.That(exists, Is.Not.EqualTo(changeDirectoryName));
    }

    [Test]
    public void CreateDirectory()
    {
        // Arrange
        Directory.CreateDirectory(_testDirectory2);

        // Act
        _service.CreateDirectory(_testDirectory2);

        // Assert
        Assert.That(Directory.Exists(_testDirectory2));
    }

    [Test]
    public void RemoveAllDirectoriesExcept()
    {
        // Act
        _service.RemoveAllDirectoriesExcept(_testDirectory, _innerTestDirectory2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(!Directory.Exists(_innerTestDirectory1));
            Assert.That(Directory.Exists(_innerTestDirectory2));
            Assert.That(!Directory.Exists(_innerTestDirectory3));
        });
    }

    [Test]
    public void MoveDirectory()
    {
        // Arrange
        var fixture = new Fixture();
        string source = Path.Combine(_testDirectory, fixture.Create<string>());
        string destination = Path.Combine(_testDirectory, fixture.Create<string>());
        Directory.CreateDirectory(source);

        // Act
        _service.MoveDirectory(source, destination);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(!Directory.Exists(source));
            Assert.That(Directory.Exists(destination));
        });
    }

    [TestCase("*11*", 2)]
    [TestCase("*22*", 1)]
    [TestCase("*ab*", 1)]
    [TestCase("*bc*", 2)]
    [TestCase("*cd*", 2)]
    [TestCase("*de*", 1)]
    [TestCase("*1*", 3)]
    [TestCase("*2*", 2)]
    [TestCase("*3*", 1)]
    public void GetFoldersGetsTopFoldersOnly(string pattern, int foldersCount)
    {
        // Act
        var folders = _service.GetFolders(_testDirectory, pattern);

        // Arrange
        Assert.That(folders, Has.Count.EqualTo(foldersCount));
    }

    [Test]
    public void WriteTextToFile()
    {
        // Arrange
        var fixture = new Fixture();
        string text1 = fixture.Create<string>();
        string text2 = fixture.Create<string>();

        // Act
        _service.WriteTextToFile(_testFile2, text1);
        _service.WriteTextToFile(_testFile2, text2);

        // Assert
        string text = File.ReadAllText(_testFile2);
        Assert.That(text, Is.EqualTo(text2));
    }
}
