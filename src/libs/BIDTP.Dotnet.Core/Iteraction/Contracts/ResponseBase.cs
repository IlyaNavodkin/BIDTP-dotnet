using BIDTP.Dotnet.Core.Iteraction.Enums;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts;

public abstract class ResponseBase : TransmitionObject
{
    protected ResponseBase(StatusCode statusCode = StatusCode.ServerError)
    {
        StatusCode = statusCode;
    }

    public StatusCode StatusCode { get; }
}