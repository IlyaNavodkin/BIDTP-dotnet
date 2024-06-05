namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteWriter
{
    Task Write(byte[] stream, Stream streamToWrite);
}