using System.IO.Compression;

namespace SimpleBackup.Abstractions;

public interface IZipWrapper
{
    public delegate CompressionLevel CompressionLevelFunc(string fileName);

    void CompressFile(string zipFile, string sourceFileName, CompressionLevel compressionLevel);
    void CompressDirectory(string zipFile, string sourceDirectory, CompressionLevel compressionLevel);
    void CompressDirectory(string zipFile, string sourceDirectory, CompressionLevelFunc getCompressionLevel);
}
