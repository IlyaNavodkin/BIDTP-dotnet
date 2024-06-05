using Lib.Iteraction.Contracts;

namespace Lib.Iteraction.Mutation.Contracts;

public interface IPreparer
{
    RequestBase PrepareRequest(RequestBase request);
    ResponseBase PrepareResponse(ResponseBase response);
}