using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;

public interface IValidator
{
    RequestBase ValidateRequest(RequestBase request);
    ResponseBase ValidateResponse(ResponseBase response);
}