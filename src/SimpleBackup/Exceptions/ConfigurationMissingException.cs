namespace SimpleBackup.Exceptions;

public sealed class ConfigurationMissingException : Exception
{
    public ConfigurationMissingException()
        : base("Configuration missing or invalid")
    {
    }
}