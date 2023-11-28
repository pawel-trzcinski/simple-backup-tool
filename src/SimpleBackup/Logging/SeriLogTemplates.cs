
using Serilog.Events;

namespace SimpleBackup.Logging;

public static class SeriLogTemplates
{
    // TODO - unit tests
    private const string DEFAULT_TEMPLATE = "{Timestamp:HH:mm:ss} {Level:u3} {Message}{NewLine}{Exception}";
    private const string VERBOSE_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Message}{NewLine}{Exception}";

    public static string GetTemplate(LogEventLevel level)
    {
        if (level == LogEventLevel.Verbose || level == LogEventLevel.Debug)
        {
            return VERBOSE_TEMPLATE;
        }

        return DEFAULT_TEMPLATE;
    }
}
