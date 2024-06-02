namespace Lib.Iteraction.ByteWriter;

public class ByteWriter : IByteWriter
{
    public async Task Write(byte[] bytes, Stream clientPipeStream)
    {        
        await clientPipeStream.WriteAsync(bytes);
    }
}