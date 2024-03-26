using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Helpers;

namespace BIDTP.Dotnet.Core.Iteraction.Options;

/// <summary>
///  Options for the SIDTPClient 
/// </summary>
public class ClientOptions
{
    /// <summary>
    ///  The json serializer options
    /// </summary>
    public readonly JsonSerializerOptions JsonSerializerOptions;
    /// <summary>
    ///  The name of the pipe 
    /// </summary>
    public readonly string ServerName;
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
    /// <param name="serverName"> The name of the pipe </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="lifeCheckTimeRate"> The time rate of the life check </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    /// <param name="connectTimeout"> The timeout of the connect </param>
    /// <param name="jsonSerializerOptions"> The json serializer options </param>
    public ClientOptions(string serverName = "defaultPipeName", 
        int chunkSize = 1024, 
        int lifeCheckTimeRate = 1000, 
        int reconnectTimeRate = 5000,
        int connectTimeout = 30000,
        JsonSerializerOptions jsonSerializerOptions = null)
    {
        JsonSerializerOptions = jsonSerializerOptions ?? JsonHelper.GetDefaultJsonSerializerOptions();
        ServerName = serverName;
        ChunkSize = chunkSize;
        LifeCheckTimeRate = lifeCheckTimeRate;
        ReconnectTimeRate = reconnectTimeRate;
        ConnectTimeout = connectTimeout;
    }
}