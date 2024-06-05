using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes;

public class ByteReader : IByteReader
{
    public async Task<byte[]> Read(Stream stream)
    {
        var contentLength = new byte[4];
        var readAsync = await stream.ReadAsync(contentLength, 0, 4);

        var contentBytes = new byte[BitConverter.ToInt32(contentLength, 0)];
        var async = await stream.ReadAsync(contentBytes, 0, contentBytes.Length);

        return contentBytes;
    }
}