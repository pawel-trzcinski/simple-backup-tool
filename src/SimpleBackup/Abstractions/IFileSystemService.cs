namespace SimpleBackup.Abstractions;

public interface IFileSystemService
{
    FileSystemEntity RecognizeEntity(string source);

    bool DirectoryExists(string directory);
    void CreateDirectory(string directory);
    void RemoveAllDirectoriesExcept(string directory, string exception);
    void MoveDirectory(string source, string destination);
    IReadOnlyCollection<string> GetFolders(string directory, string pattern);

    void WriteTextToFile(string path, string contents);
}