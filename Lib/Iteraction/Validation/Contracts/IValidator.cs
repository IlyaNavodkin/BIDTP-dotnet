using Lib.Iteraction.Contracts;

namespace Lib.Iteraction.Validation.Contracts;

public interface IValidator
{
    RequestBase ValidateRequest(RequestBase request);
    ResponseBase ValidateResponse(ResponseBase response);
}