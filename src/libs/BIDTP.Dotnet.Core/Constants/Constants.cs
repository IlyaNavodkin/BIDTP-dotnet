namespace BIDTP.Dotnet.Core.Constants;

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