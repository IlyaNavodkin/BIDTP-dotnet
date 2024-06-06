using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using System;
using System.Diagnostics;

namespace BIDTP.Dotnet.Core.Iteraction.Mutation;

/// <summary>
///  Constants for the protocol 
/// </summary>
public static class Constants
{
    /// <summary>
    ///  The name of the protocol 
    /// </summary>
    public const string ProtocolName = "BIDTP";
    /// <summary>
    ///  The full name of the protocol 
    /// </summary>
    public const string ProtocolFullName = "Bidirectional Interprocess Data Transfer Protocol";
    /// <summary>
    ///  The version of the protocol
    /// </summary>
    public const string ProtocolVersion = "1.0";

    /// <summary>
    ///  The name of the "Route" header 
    /// </summary>
    public const string RouteHeaderName = "Route";
    /// <summary>
    ///  The name of the "ProtocolName" header
    /// </summary>
    public const string ProtocolHeaderName = "ProtocolName";
    /// <summary>
    ///  The name of the "ProtocolFullName" header
    /// </summary>
    public const string ProtocolFullNameHeaderName = "ProtocolFullName";
    /// <summary>
    ///  The name of the "ProtocolVersion" header
    /// </summary>
    public const string ProtocolVersionHeaderName = "ProtocolVersion";
    /// <summary>
    ///  The name of the "ResponseProcessId" header
    /// </summary>
    public const string ResponseProcessIdHeaderName = "ResponseProcessId";
    /// <summary>
    ///  The name of the "ResponseProcessId" header
    /// </summary>
    public const string RequestResponseGuidHeaderName = "Guid";
}

public class Preparer : IPreparer
{
    public RequestBase PrepareRequest(RequestBase request)
    {
        request.Headers.Add(Constants.ProtocolHeaderName, Constants.ProtocolName);
        request.Headers.Add(Constants.ProtocolFullNameHeaderName, Constants.ProtocolFullName);
        request.Headers.Add(Constants.ProtocolVersionHeaderName, Constants.ProtocolVersion);
        request.Headers.Add(Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
        request.Headers.Add(Constants.RequestResponseGuidHeaderName, Guid.NewGuid().ToString());

        return request;
    }

    public ResponseBase PrepareResponse(ResponseBase response)
    {
        response.Headers.Add(Constants.ProtocolHeaderName, Constants.ProtocolName);
        response.Headers.Add(Constants.ProtocolFullNameHeaderName, Constants.ProtocolFullName);
        response.Headers.Add(Constants.ProtocolVersionHeaderName, Constants.ProtocolVersion);
        response.Headers.Add(Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
        response.Headers.Add(Constants.RequestResponseGuidHeaderName, Guid.NewGuid().ToString());

        return response;
    }
}