using BIDTP.Dotnet.Core.Request;
using BIDTP.Dotnet.Core.Response;

namespace BIDTP.Dotnet.Server.Server.Iteraction;

/// <summary>
///  The context of the server in the request scope
/// </summary>
public class Context
{
    /// <summary>
    ///  The request from the client
    /// </summary>
    public Request Request { get; }
    /// <summary>
    ///  The response from the server
    /// </summary>
    public Response Response { get; set; }
    /// <summary>
    ///  The service provider of the server
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="request"> The request.</param>
    /// <param name="pipeStream"> The pipe stream.</param>
    /// <param name="serviceProvider"> The service provider.</param>
    /// <exception cref="ArgumentNullException"> request -or- pipeStream -or- serviceProvider</exception>
    public Context(Request request, IServiceProvider serviceProvider)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
}