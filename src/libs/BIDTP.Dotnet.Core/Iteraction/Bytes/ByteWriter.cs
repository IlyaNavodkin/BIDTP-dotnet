using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes;

public class ByteWriter : IByteWriter
{
    public async Task Write(byte[] bytes, Stream clientPipeStream)
    {
        await clientPipeStream.WriteAsync(bytes);
    }
}