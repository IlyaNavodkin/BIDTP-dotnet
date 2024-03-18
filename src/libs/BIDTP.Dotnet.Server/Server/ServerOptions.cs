namespace BIDTP.Dotnet.Server.Server;

/// <summary>
///  The options of the server 
/// </summary>
public class ServerOptions
{
    /// <summary>
    ///  The name of the pipe 
    /// </summary>
    public readonly string PipeName;
    /// <summary>
    ///  The chunk size for the transmission data
    /// </summary>
    public readonly int ChunkSize;
    /// <summary>
    ///  The time rate of the life check 
    /// </summary>
    public readonly int ReconnectTimeRate;

    /// <summary>
    ///  Create a new instance of the ServerOptions
    /// </summary>
    /// <param name="pipeName"> The name of the pipe </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    public ServerOptions(string pipeName, int chunkSize, int reconnectTimeRate)
    {
        PipeName = pipeName;
        ChunkSize = chunkSize;
        ReconnectTimeRate = reconnectTimeRate;
    }
}