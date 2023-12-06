using SimpleBackup.Configuration;

namespace SimpleBackup.Engine.Compressors;

public interface ICompressorFactory
{
    ICompressor Create(CompressionType compressionType);
}
