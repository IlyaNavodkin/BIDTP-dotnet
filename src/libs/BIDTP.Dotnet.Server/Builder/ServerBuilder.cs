using System.Security.Cryptography;
using System.Text;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Iteraction;
using Microsoft.Extensions.DependencyInjection;

namespace BIDTP.Dotnet.Builder;

/// <summary>
///  The server builder of SIDTP protocol
/// </summary>
public class ServerBuilder
{
    private readonly Dictionary<string, Func<Context, Task>[]> _routeHandlers 
        = new Dictionary<string, Func<Context, Task>[]>();
    
    /// <summary>
    ///  The service collection of the server 
    /// </summary>
    public readonly IServiceCollection ServiceCollection;

    private ServerOptions _options;

    /// <summary>
    ///  Constructor
    /// </summary>
    public ServerBuilder()
    {
        ServiceCollection = new ServiceCollection();
    }
    
    /// <summary>
    ///  Add a route handler or a group of handlers to the server
    /// </summary>
    /// <param name="route"> The route. </param>
    /// <param name="handlers"> The handler or array of handlers.</param>
    /// <returns> The server builder. </returns>
    public ServerBuilder AddRoute(string route, params Func<Context, Task>[] handlers)
    {
        _routeHandlers.Add(route, handlers);
        
        return this;
    }

    /// <summary>
    ///  Set the general options of the server
    /// </summary>
    /// <param name="options"> The options. </param>
    /// <returns> The server builder. </returns>
    public ServerBuilder SetGeneralOptions(ServerOptions options)
    {
        _options = options;
        
        return this;
    }
    
    /// <summary>
    ///  Build the server and return it 
    /// </summary>
    /// <returns> The server. </returns>
    /// <exception cref="InvalidOperationException"> PipeStream must be provided. </exception>
    public Dotnet.Server Build()
    {
        if (_routeHandlers.Count == 0) throw new InvalidOperationException("Route handlers must be provided");
        var buildServiceProvider = ServiceCollection.BuildServiceProvider();
        var result =  new Dotnet.Server(_options.PipeName, _options.ChunkSize, _options.ReconnectTimeRate, _routeHandlers, buildServiceProvider);
        
        return result;
    }
    
    /// <summary>
    ///  Get the hashed pipe name of the server. Recommended for use.
    /// </summary>
    /// <returns> The hashed pipe name. </returns>
    public static string GetHashedPipeName()
    {
        var sha256Managed = new SHA256Managed();
        
        var serverDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var pipeNameInput = $"{Environment.UserName}.{serverDirectory}";
        
        var hash = sha256Managed.ComputeHash(Encoding.Unicode.GetBytes(pipeNameInput));
        
        var name = Convert.ToBase64String(hash)
            .Replace("/", "_")
            .Replace("=", string.Empty);
        
        return name;
    }
}