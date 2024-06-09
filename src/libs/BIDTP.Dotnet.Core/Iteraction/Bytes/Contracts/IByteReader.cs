using System;
using System.IO;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Events;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteReader
{
    Task<byte[]> Read(Stream stream);

    /// <summary>
    ///  Event handler for progress of read operations
    /// </summary>
    public event EventHandler<ProgressEventArgs> ReadProgress;

    /// <summary>
    ///  Occurs when read progress
    /// </summary>
    public event EventHandler<EventArgs> ReadCompleted;
}