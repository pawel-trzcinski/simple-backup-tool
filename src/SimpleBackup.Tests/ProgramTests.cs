using NUnit.Framework;
using SimpleInjector;

namespace SimpleBackup.Tests;

[TestFixture]
public class ProgramTests
{
    [Test]
    public void ContainerRegistrationDoesNotThrow()
    {
        Assert.DoesNotThrow(() => Program.RegisterAndVerify(new Container()));
    }
}
