namespace BIDTP.Dotnet.Server.Errors;

/// <summary>
/// Internal server error type
/// </summary>
public enum InternalServerErrorType
{
    /// <summary>
    ///  Dispatcher request error in server
    /// </summary>
    DispatcherExceptionError = 1,
    /// <summary>
    ///  Route not found error
    /// </summary>
    RouteNotFoundError = 2,
}