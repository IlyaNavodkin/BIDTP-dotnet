namespace Lib.Iteraction.ByteReader;

public interface IByteReader
{
    Task<MemoryStream> Read(Stream stream);
}