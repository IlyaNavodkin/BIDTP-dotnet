namespace BIDTP.Dotnet;

/// <summary>
/// Internal server error type
/// </summary>
public enum InternalServerErrorType
{
    /// <summary>
    ///  Dispatcher request error in server
    /// </summary>
    DispatcherExceptionError,
    /// <summary>
    ///  Route not found error
    /// </summary>
    RouteNotFoundError,
    /// <summary>
    ///  Unknown error in server
    /// </summary>
    UnknownError
}