namespace Lib.Iteraction.ByteWriter;

public interface IByteWriter
{
     Task Write(MemoryStream stream, Stream streamToWrite);
}