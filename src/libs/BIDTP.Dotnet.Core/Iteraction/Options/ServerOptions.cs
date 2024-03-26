using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Helpers;

namespace BIDTP.Dotnet.Core.Iteraction.Options;

/// <summary>
///  The options of the server 
/// </summary>
public class ServerOptions
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
    public readonly int ReconnectTimeRate;

    /// <summary>
    ///  Create a new instance of the ServerOptions
    /// </summary>
    /// <param name="serverName"> The name of the server </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    /// <param name="jsonSerializerOptions"> The json serializer options </param>
    public ServerOptions(
        string serverName = "defaultServerName", 
        int chunkSize = 1024, 
        int reconnectTimeRate = 5000, 
        JsonSerializerOptions jsonSerializerOptions = null
        )
    {
        ServerName = serverName;
        ChunkSize = chunkSize;
        ReconnectTimeRate = reconnectTimeRate;
        JsonSerializerOptions = jsonSerializerOptions 
        ?? JsonHelper.GetDefaultJsonSerializerOptions();
    }
}