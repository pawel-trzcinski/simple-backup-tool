namespace SimpleBackup.Abstractions;

public interface IFileSystemService
{
    FileSystemEntity RecognizeEntity(string source);

    bool DirectoryExists(string directory);
    void CreateDirectory(string directory);

    void WriteTextToFile(string path, string contents);
}