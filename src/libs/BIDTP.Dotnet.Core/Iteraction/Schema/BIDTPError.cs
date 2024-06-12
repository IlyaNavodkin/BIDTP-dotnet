namespace BIDTP.Dotnet.Core.Iteraction.Schema;

public class BIDTPError
{
    public string Message { get; set; }
    public string Description { get; set; }
    public string StackTrace { get; set; }
    public int ErrorCode { get; set; }
}