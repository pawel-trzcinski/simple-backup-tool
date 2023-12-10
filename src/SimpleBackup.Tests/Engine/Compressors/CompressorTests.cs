using NSubstitute;
using NUnit.Framework;
using Serilog;
using SimpleBackup.Abstractions;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors
{
    [TestFixture]
    public class CompressorTests
    {
        private static readonly ILogger logger = Substitute.For<ILogger>();
        private IFileSystemService fileSystemService = Substitute.For<IFileSystemService>();
        private IZipWrapper zipWrapper = Substitute.For<IZipWrapper>();
        private IArchiveNameService archiveNameService = Substitute.For<IArchiveNameService>();

        [SetUp]
        public void SetUp()
        {
            fileSystemService = Substitute.For<IFileSystemService>();
            zipWrapper = Substitute.For<IZipWrapper>();
            archiveNameService = Substitute.For<IArchiveNameService>();
        }

        // TODO
    }
}
