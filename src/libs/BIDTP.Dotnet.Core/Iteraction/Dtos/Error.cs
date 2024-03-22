namespace BIDTP.Dotnet.Iteraction.Dtos;

/// <summary>
///  The error of the response 
/// </summary>
public class Error
{
    /// <summary>
    ///  The message of the error 
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    ///  The description of the error 
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    ///  The stack trace of the error 
    /// </summary>
    public string StackTrace { get; set; }
    /// <summary>
    ///  The error code of the error 
    /// </summary>
    public int ErrorCode { get; set; }
}