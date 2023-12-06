using System.Text;
using Serilog;

namespace SimpleBackup.Abstractions;

public class FileSystemService(ILogger logger) : IFileSystemService
{
    public FileSystemEntity RecognizeEntity(string source)
    {
        if (File.Exists(source))
        {
            return new FileSystemEntity(FileSystemEntityType.File, source);
        }

        if (DirectoryExists(source))
        {
            return new FileSystemEntity(FileSystemEntityType.Direcotry, source);
        }

        throw new IOException($"Unable to find file or folder '{source}'");
    }

    public bool DirectoryExists(string directory)
    {
        return Directory.Exists(directory);
    }

    public void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
    }

    public void RemoveAllDirectoriesExcept(string directory, string exception)
    {
        foreach (string directoryToDelete in Directory.GetDirectories(directory))
        {
            if (directoryToDelete.Equals(exception))
            {
                continue;
            }

            logger.Information($"Deleting direcotry {directoryToDelete}");
            Directory.Delete(directoryToDelete, true);
        }
    }

    public void MoveDirectory(string source, string destination)
    {
        Directory.Move(source, destination);
    }

    public IReadOnlyCollection<string> GetFolders(string directory, string pattern)
    {
        return Directory.GetDirectories(directory, pattern, SearchOption.TopDirectoryOnly);
    }

    public void WriteTextToFile(string path, string contents)
    {
        File.WriteAllText(path, contents, Encoding.UTF8);
    }
}
