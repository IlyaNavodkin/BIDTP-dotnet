using System;

namespace BIDTP.Dotnet.Events;

/// <summary>
///  The progress event args
/// </summary>
public class ProgressEventArgs : EventArgs
{
    /// <summary>
    ///  The progress is complete
    /// </summary>
    public bool IsComplete { get; }
    /// <summary>
    ///  The bytes written
    /// </summary>
    public int BytesWritten { get; }
    /// <summary>
    ///  The total bytes
    /// </summary>
    public int TotalBytes { get; }
    /// <summary>
    ///  The progress operation type
    /// </summary>
    public ProgressOperationType ProgressOperationType { get; }

    /// <summary>
    ///  Create a new progress event
    /// </summary>
    /// <param name="bytesWritten"> The bytes written </param>
    /// <param name="totalBytes"> The total bytes </param>
    /// <param name="progressOperationType"></param>
    public ProgressEventArgs(int bytesWritten, int totalBytes, ProgressOperationType progressOperationType)
    {
        if (totalBytes == bytesWritten ) IsComplete = true;
        BytesWritten = bytesWritten;
        TotalBytes = totalBytes;
        ProgressOperationType = progressOperationType;
    }
}