using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Handle;

/// <summary>
///  The context of the server in the request scope
/// </summary>
public class Context
{
    /// <summary>
    ///  The request from the client
    /// </summary>
    public RequestBase Request { get; }
    /// <summary>
    ///  The response from the server
    /// </summary>
    public ResponseBase Response { get; set; }
    /// <summary>
    ///  The service provider of the server
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
    /// <summary>
    ///  Additional data of the request for mutation 
    /// </summary>
    public ObjectContainer ObjectContainer { get; } = new ObjectContainer();

    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="request"> The request.</param>
    /// <param name="serviceProvider"> The service provider.</param>
    /// <exception cref="ArgumentNullException"> request -or- pipeStream -or- serviceProvider</exception>
    public Context(RequestBase request,
        IServiceProvider serviceProvider)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ServiceProvider = serviceProvider;
    }
}
