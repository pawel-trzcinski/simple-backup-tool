using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using SimpleBackup.Engine.Compressors;

namespace SimpleBackup.Tests.Engine.Compressors
{
    [TestFixture]
    public class ThresholdGuardTests
    {
        private IArchiveNameService _archiveNameService = Substitute.For<IArchiveNameService>();

        [SetUp]
        public void SetUp()
        {
            _archiveNameService = Substitute.For<IArchiveNameService>();
        }

        [Test]
        public void ThresholdPassed()
        {
            // Arrange
            var fixture = new Fixture();
            var mainFolder = fixture.Create<string>();
            var threshold = TimeSpan.FromMilliseconds(2);
            var timePassed = TimeSpan.FromMilliseconds(3);

            _archiveNameService.GetTimePassedFromLatesFinishedArchive(mainFolder).Returns(timePassed);

            var guard = new ThresholdGuard(_archiveNameService);

            // Act
            bool allowed = guard.RefreshThresholdPassed(mainFolder, threshold);

            // Assert
            Assert.That(allowed);
        }

        [Test]
        public void ThresholdGuarded()
        {
            // Arrange
            var fixture = new Fixture();
            var mainFolder = fixture.Create<string>();
            var threshold = TimeSpan.FromMilliseconds(2);
            var timePassed = TimeSpan.FromMilliseconds(1);

            _archiveNameService.GetTimePassedFromLatesFinishedArchive(mainFolder).Returns(timePassed);

            var guard = new ThresholdGuard(_archiveNameService);

            // Act
            bool allowed = guard.RefreshThresholdPassed(mainFolder, threshold);

            // Assert
            Assert.That(allowed, Is.False);
        }
    }
}
