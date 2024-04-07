namespace Lib;

/// <summary>
///  The status code of the response
/// </summary>
public enum StatusCode
{
    /// <summary>
    ///  The request was successful
    /// </summary>
    Success = 100, 
    
    /// <summary>
    ///  The request was not successful - bad request from the client
    /// </summary>
    ClientError = 200,
    
    /// <summary>
    ///  The request was not successful - unauthorized
    /// </summary>
    Unauthorized = 201,
    
    /// <summary>
    ///  The request was not successful - route is not found 
    /// </summary>
    NotFound = 202,
    
    /// <summary>
    ///  The request was not successful - internal server error
    /// </summary>
    ServerError = 300
}