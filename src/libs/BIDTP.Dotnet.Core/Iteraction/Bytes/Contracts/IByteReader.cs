namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteReader
{
    Task<byte[]> Read(Stream stream);
}