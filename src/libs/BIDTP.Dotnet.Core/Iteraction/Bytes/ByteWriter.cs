using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes;

public class ByteWriter : IByteWriter
{
    public async Task Write(byte[] bytes, Stream clientPipeStream)
    {
        await clientPipeStream.WriteAsync(bytes, 0, bytes.Length);
    }
}