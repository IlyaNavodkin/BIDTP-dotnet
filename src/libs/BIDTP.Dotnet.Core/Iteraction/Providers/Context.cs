using System;
using System.Collections.Generic;
using BIDTP.Dotnet.Core.Iteraction.Dtos;

namespace BIDTP.Dotnet.Core.Iteraction.Providers;

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
    ///  Additional data of the request for mutation 
    /// </summary>
    public Dictionary<Type, object[]> AdditionalData { get; } = new Dictionary<Type, object[]>();

    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="request"> The request.</param>
    /// <param name="serviceProvider"> The service provider.</param>
    /// <exception cref="ArgumentNullException"> request -or- pipeStream -or- serviceProvider</exception>
    public Context(Request request, IServiceProvider serviceProvider)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
}