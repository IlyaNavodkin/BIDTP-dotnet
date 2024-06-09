using System;
using System.IO;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Events;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteWriter
{
    /// <summary>
    ///  Event handler for progress of write operations 
    /// </summary>
    public event EventHandler<ProgressEventArgs> WriteProgress;

    /// <summary>
    ///  Occurs when write progress
    /// </summary>
    public event EventHandler<EventArgs> WriteCompleted;

    Task Write(byte[] stream, Stream streamToWrite);
}