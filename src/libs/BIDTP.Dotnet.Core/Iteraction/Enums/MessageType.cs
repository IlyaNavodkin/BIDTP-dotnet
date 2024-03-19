namespace BIDTP.Dotnet.Iteraction.Enums;

/// <summary>
///  The type of the message
/// </summary>
public enum MessageType
{
    /// <summary>
    ///  Health check of the server. Used for health check of the client
    /// </summary>
    HealthCheck = 1,
    /// <summary>
    ///  The general type of the message 
    /// </summary>
    General = 2
}