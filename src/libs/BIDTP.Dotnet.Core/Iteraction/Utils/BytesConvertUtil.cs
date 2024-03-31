using System;
using System.IO;
using System.Text;
using System.Threading;

namespace BIDTP.Dotnet.Core.Iteraction.Utils;

/// <summary>
///  Convert bytes to string
/// </summary>
public static class BytesConvertUtil
{
    /// <summary>
    ///  Read string bytes from the stream
    /// </summary>
    /// <param name="cancellationToken"> The cancellation token. </param>
    /// <param name="binaryReader"> The binary reader. </param>
    /// <param name="bytesReadsCount"> The bytes read count. </param>
    /// <param name="totalBytesReadCount"> The total bytes read count. </param>
    /// <param name="encoding"> The encoding. </param>
    /// <param name="chunkSize"> The chunk size. </param>
    /// <param name="progressReadChangeAction"> The progress read change action. </param>
    /// <returns></returns>
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

    /// <summary>
    ///  Write string bytes to the stream 
    /// </summary>
    /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
    /// <param name="binaryWriter"> The binary writer. </param>
    /// <param name="bytesToWrite"> The bytes to write. </param>
    /// <param name="bytesWriteCount"> The bytes write count. </param>
    /// <param name="totalBytesWriteCount"> The total bytes write count. </param>
    /// <param name="chunkSize"> The chunk size. </param>
    /// <param name="progressWriteChangeAction"> The progress write change action. </param>
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