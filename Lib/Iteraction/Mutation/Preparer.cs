using Lib.Iteraction.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using System.Diagnostics;

namespace Lib.Iteraction.Mutation;

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
}

public class Preparer : IPreparer
{
    public RequestBase PrepareRequest(RequestBase request)
    {
        request.Headers.Add(Constants.ProtocolHeaderName, Constants.ProtocolName);
        request.Headers.Add(Constants.ProtocolFullNameHeaderName, Constants.ProtocolFullName);
        request.Headers.Add(Constants.ProtocolVersionHeaderName, Constants.ProtocolVersion);
        request.Headers.Add(Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());

        return request;
    }

    public ResponseBase PrepareResponse(ResponseBase response)
    {
        response.Headers.Add("Prepared", "true");

        return response;
    }
}