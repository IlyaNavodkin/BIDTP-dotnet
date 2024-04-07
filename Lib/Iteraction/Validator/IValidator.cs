using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Validator;

public interface IValidator
{
    RequestBase ValidateRequest(RequestBase request);
    ResponseBase ValidateResponse(ResponseBase response);
}