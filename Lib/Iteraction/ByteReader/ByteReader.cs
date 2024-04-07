namespace Lib.Iteraction.ByteReader;

public class ByteReader : IByteReader
{
    public async Task<MemoryStream> Read(Stream stream)
    {
        // var binaryReader = new BinaryReader(stream);
        //
        // var contentLenght = binaryReader.ReadInt32();
        //
        // var contentBytes = binaryReader.ReadBytes(contentLenght);
        var contentLength = new byte[4];
        var readAsync = await stream.ReadAsync(contentLength, 0, 4);
        
        var contentBytes = new byte[BitConverter.ToInt32(contentLength, 0)];
        var async = await stream.ReadAsync(contentBytes, 0, contentBytes.Length);

        var memoryStream = new MemoryStream(contentBytes);

        return memoryStream;
    }
}