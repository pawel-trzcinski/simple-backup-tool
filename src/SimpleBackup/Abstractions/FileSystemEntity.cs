namespace SimpleBackup.Abstractions;

public class FileSystemEntity(FileSystemEntityType type, string source, string name)
{
    public  FileSystemEntityType Type { get; } = type;
    public string Source { get; } = source;
    public string Name { get; } = name;
}
