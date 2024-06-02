namespace Lib.Iteraction.ByteReader;

public interface IByteReader
{
    Task<byte[]> Read(Stream stream);
}