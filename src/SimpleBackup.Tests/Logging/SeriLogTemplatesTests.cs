using NUnit.Framework;
using Serilog.Events;
using SimpleBackup.Logging;

namespace SimpleBackup.Tests.Logging
{
    [TestFixture]
    public class SeriLogTemplatesTests
    {
        [TestCase(LogEventLevel.Verbose, SeriLogTemplates.VERBOSE_TEMPLATE)]
        [TestCase(LogEventLevel.Debug, SeriLogTemplates.VERBOSE_TEMPLATE)]
        [TestCase(LogEventLevel.Information, SeriLogTemplates.DEFAULT_TEMPLATE)]
        [TestCase(LogEventLevel.Warning, SeriLogTemplates.DEFAULT_TEMPLATE)]
        [TestCase(LogEventLevel.Error, SeriLogTemplates.DEFAULT_TEMPLATE)]
        [TestCase(LogEventLevel.Fatal, SeriLogTemplates.DEFAULT_TEMPLATE)]
        public void CorrectTemplateReturned(LogEventLevel level, string expectedTemplate)
        {
            // Act
            string actualTemplate = SeriLogTemplates.GetTemplate(level);

            // Assert
            Assert.That(actualTemplate, Is.EqualTo(expectedTemplate));
        }
    }
}
