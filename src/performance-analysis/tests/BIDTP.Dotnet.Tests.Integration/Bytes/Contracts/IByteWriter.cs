namespace Lib.Iteraction.Bytes.Contracts;

public interface IByteWriter
{
    Task Write(byte[] stream, Stream streamToWrite);
}