namespace Lib.Iteraction.Bytes.Contracts;

public interface IByteReader
{
    Task<byte[]> Read(Stream stream);
}