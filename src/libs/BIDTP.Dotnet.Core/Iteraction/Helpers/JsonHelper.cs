using System.Text.Encodings.Web;
using System.Text.Json;

namespace BIDTP.Dotnet.Core.Iteraction.Helpers;

/// <summary>
///  Json helper 
/// </summary>
public static class JsonHelper
{
    /// <summary>
    ///  Get the json serializer options
    /// </summary>
    /// <returns> The json serializer options </returns>
    public static JsonSerializerOptions GetDefaultJsonSerializerOptions() 
        => new() { 
            Encoder = JavaScriptEncoder.Default
        };
}