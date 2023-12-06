using System.IO.Compression;
using Serilog;

namespace SimpleBackup.Abstractions;

public sealed class ZipWrapper(ILogger logger) : IZipWrapper
{
    public void CompressFile(string zipFile, string sourceFileName, CompressionLevel compressionLevel)
    {
        using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
        {
            archive.CreateEntryFromFile(sourceFileName, Path.GetFileName(sourceFileName), compressionLevel);
        }
    }

    public void CompressDirectory(string zipFile, string sourceDirectory, CompressionLevel compressionLevel)
    {
        ZipFile.CreateFromDirectory(sourceDirectory, zipFile, compressionLevel, true);
    }

    public void CompressDirectory(string zipFile, string sourceDirectory, IZipWrapper.CompressionLevelFunc getCompressionLevel)
    {
        logger.Information($"Fetching files from {sourceDirectory}");
        string[] files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
        logger.Information($"Compressing adaptively files of count: {files.Length}");

        const int MAX_STEPS = 100;
        int step = files.Length / MAX_STEPS + 1;

        using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
        {
            int index = 0;
            foreach (string file in files)
            {
                string sourceDirectoryName = Path.GetFileName(sourceDirectory);
                string entryName = Path.Combine(sourceDirectoryName, file.Substring(sourceDirectory.Length).Trim(Path.PathSeparator, Path.VolumeSeparatorChar, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                entryName = entryName.Replace('\\', '/');

                archive.CreateEntryFromFile(file, entryName, getCompressionLevel(file));

                ++index;
                if (index % step == 0)
                {
                    logger.Information($"Progress: {index / step}%");
                }
            }
        }
    }
}
