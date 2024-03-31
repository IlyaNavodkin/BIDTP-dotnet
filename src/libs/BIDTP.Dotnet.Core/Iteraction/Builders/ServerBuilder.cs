using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Core.Iteraction.Providers;

namespace BIDTP.Dotnet.Core.Iteraction.Builders;

/// <summary>
///  The server builder of SIDTP protocol
/// </summary>
public class ServerBuilder
{
    private readonly Dictionary<string, Func<Context, Task>[]> _routeHandlers 
        = new Dictionary<string, Func<Context, Task>[]>();
    private ServerOptions _options;
    
    /// <summary>
    ///  The service provider
    /// </summary>
    private IServiceProvider _serviceProvider;
    
    /// <summary>
    ///  Constructor
    /// </summary>
    public ServerBuilder AddDiContainer(IServiceProvider serviceCollection)
    {
        _serviceProvider = serviceCollection;
        
        return this;
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
    public Server Build()
    {
        if (_options == null)
        {
            throw new InvalidOperationException("Server options must be provided");
        }
        
        var result = new Server(_options, _routeHandlers, _serviceProvider);
        
        return result;
    }
}