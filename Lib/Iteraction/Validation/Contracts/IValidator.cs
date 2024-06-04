using Lib.Iteraction.Contracts;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Validation.Contracts;

public interface IValidator
{
    RequestBase ValidateRequest(RequestBase request);
    ResponseBase ValidateResponse(ResponseBase response);
}