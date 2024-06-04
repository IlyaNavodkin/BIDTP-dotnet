using Lib.Iteraction.Bytes.Contracts;

namespace Lib.Iteraction.Bytes;

public class ByteWriter : IByteWriter
{
    public async Task Write(byte[] bytes, Stream clientPipeStream)
    {        
        await clientPipeStream.WriteAsync(bytes);
    }
}