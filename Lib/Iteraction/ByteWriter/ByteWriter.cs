namespace Lib.Iteraction.ByteWriter;

public class ByteWriter : IByteWriter
{
    public async Task Write(MemoryStream stream, Stream clientPipeStream)
    {
        var buffer = stream.ToArray();
        
        await clientPipeStream.WriteAsync(buffer);
    }
}