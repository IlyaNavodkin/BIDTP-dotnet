using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes;

public class ByteWriter : IByteWriter
{
    /// <inheritdoc/>
    public event EventHandler<ProgressEventArgs> WriteProgress;

    /// <inheritdoc/>
    public event EventHandler<EventArgs> WriteCompleted;

    public async Task Write(byte[] bytes, Stream clientPipeStream)
    {
        await clientPipeStream.WriteAsync(bytes, 0, bytes.Length);
    }
}