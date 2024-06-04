using Lib.Iteraction.Contracts;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Mutation.Contracts;

public interface IPreparer
{
    RequestBase PrepareRequest(RequestBase request);
    ResponseBase PrepareResponse(ResponseBase response);
}