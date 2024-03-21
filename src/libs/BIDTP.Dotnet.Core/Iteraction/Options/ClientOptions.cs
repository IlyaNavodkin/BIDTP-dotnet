namespace BIDTP.Dotnet.Iteraction.Options;

/// <summary>
///  Options for the SIDTPClient 
/// </summary>
public class ClientOptions
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
    public readonly int LifeCheckTimeRate;
    /// <summary>
    ///  The time rate of the reconnect 
    /// </summary>
    public readonly int ReconnectTimeRate;
    /// <summary>
    ///  The timeout of the connect
    /// </summary>
    public readonly int ConnectTimeout;

    /// <summary>
    ///  Create a new SIDTPClientOptions
    /// </summary>
    /// <param name="pipeName"> The name of the pipe </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="lifeCheckTimeRate"> The time rate of the life check </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    /// <param name="connectTimeout"> The timeout of the connect </param>
    public ClientOptions(string pipeName, int chunkSize, 
        int lifeCheckTimeRate, int reconnectTimeRate,
        int connectTimeout)
    {
        PipeName = pipeName;
        ChunkSize = chunkSize;
        LifeCheckTimeRate = lifeCheckTimeRate;
        ReconnectTimeRate = reconnectTimeRate;
        ConnectTimeout = connectTimeout;
    }
}