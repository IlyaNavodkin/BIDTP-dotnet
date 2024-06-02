namespace Lib.Iteraction.ByteWriter;

public interface IByteWriter
{
     Task Write(byte[] stream, Stream streamToWrite);
}