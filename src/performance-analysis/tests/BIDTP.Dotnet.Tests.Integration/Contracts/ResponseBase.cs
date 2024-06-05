using Lib.Iteraction.Enums;

namespace Lib.Iteraction.Contracts;

public abstract class ResponseBase : TransmitionObject
{
    protected ResponseBase(StatusCode statusCode = StatusCode.ServerError)
    {
        StatusCode = statusCode;
    }

    public StatusCode StatusCode { get; }
}