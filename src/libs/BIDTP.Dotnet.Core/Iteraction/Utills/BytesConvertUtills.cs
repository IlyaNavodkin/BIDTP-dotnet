using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace BIDTP.Dotnet.Core.Iteraction.Utills;

public static class BytesConvertUtills
{
    public static string ReadStringBytes(CancellationToken cancellationToken,
        BinaryReader binaryReader,
        ref int bytesReadsCount,
        int totalBytesReadCount,
        Encoding encoding,
        int chunkSize, 
        Action<int, int> progressReadChangeAction)
    {
        var stringBuilder = new StringBuilder();
        
        var bytesToReadLength = binaryReader.ReadInt32();
        
        bytesReadsCount += 4;
        progressReadChangeAction(bytesReadsCount, totalBytesReadCount);
        
        for (var i = 0; i < bytesToReadLength; i += chunkSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var bytesToRead = Math.Min(chunkSize , bytesToReadLength - i);
            
            var bytesReadFromStream = binaryReader.ReadBytes(bytesToRead);
            var readString = encoding.GetString(bytesReadFromStream);
                
            stringBuilder.Append(readString);
                
            bytesReadsCount += bytesReadFromStream.Length; ;
            progressReadChangeAction(bytesReadsCount, totalBytesReadCount);
        }
            
        var result = stringBuilder.ToString();
            
        return result;
    }

    public static void WriteStringBytes(
        CancellationToken cancellationToken,
        BinaryWriter binaryWriter,
        byte[] bytesToWrite,
        ref int bytesWriteCount,
        int totalBytesWriteCount,
        int chunkSize,
        Action<int, int> progressWriteChangeAction)
    {
        binaryWriter.Write(bytesToWrite.Length);

        bytesWriteCount += 4;
        progressWriteChangeAction(bytesWriteCount, totalBytesWriteCount);
        
        for (var i = 0; i < bytesToWrite.Length; i += chunkSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wroteBytes = Math.Min(chunkSize, bytesToWrite.Length - i);
            
            binaryWriter.Write(bytesToWrite, i, wroteBytes);

            bytesWriteCount += wroteBytes;
            
            progressWriteChangeAction(bytesWriteCount, totalBytesWriteCount);
        }
    }
}