using Serilog.Events;

namespace SimpleBackup.Logging;

public static class SeriLogTemplates
{
    public const string DEFAULT_TEMPLATE = "{Timestamp:HH:mm:ss} {Level:u3} {Message}{NewLine}{Exception}";
    public const string VERBOSE_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Message}{NewLine}{Exception}";

    public static string GetTemplate(LogEventLevel level)
    {
        if (level == LogEventLevel.Verbose || level == LogEventLevel.Debug)
        {
            return VERBOSE_TEMPLATE;
        }

        return DEFAULT_TEMPLATE;
    }
}
