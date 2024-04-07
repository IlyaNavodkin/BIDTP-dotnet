namespace Lib.Iteraction.Response;

public abstract class ResponseBase : TransmitionObject
{
    protected ResponseBase(StatusCode statusCode = StatusCode.ServerError)
    {
        StatusCode = statusCode;
    }

    public StatusCode StatusCode { get; }
}