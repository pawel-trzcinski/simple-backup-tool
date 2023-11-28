using System.Text;

namespace SimpleBackup.Abstractions;

public class FileSystemService : IFileSystemService
{
    // TODO - unit tests

    public FileSystemEntity RecognizeEntity(string source)
    {
        if (File.Exists(source))
        {
            return new FileSystemEntity(FileSystemEntityType.File, source, Path.GetFileName(source));
        }

        if (DirectoryExists(source))
        {
            return new FileSystemEntity(FileSystemEntityType.Direcotry, source, Path.GetFileName(source));
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

    public void WriteTextToFile(string path, string contents)
    {
        File.WriteAllText(path, contents, Encoding.UTF8);
    }
}
