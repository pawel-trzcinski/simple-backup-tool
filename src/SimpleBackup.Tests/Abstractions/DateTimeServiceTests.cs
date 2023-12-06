using NUnit.Framework;
using SimpleBackup.Abstractions;

namespace SimpleBackup.Tests.Abstractions;

[TestFixture]
public class DateTimeServiceTests
{
    [Test]
    public void NowSubsequentValues()
    {
        // Arrange
        DateTime now1 = DateTime.Now;
        var service = new DateTimeService();

        // Act
        DateTime now2 = service.Now;
        DateTime now3 = service.Now;

        // Assert
        Assert.That(now1, Is.LessThanOrEqualTo(now2));
        Assert.That(now2, Is.LessThanOrEqualTo(now3));
    }
}
