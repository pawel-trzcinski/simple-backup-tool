namespace SimpleBackup.Abstractions;

public class FileSystemEntity(FileSystemEntityType type, string source)
{
    public  FileSystemEntityType Type { get; } = type;
    public string Source { get; } = source;
}
