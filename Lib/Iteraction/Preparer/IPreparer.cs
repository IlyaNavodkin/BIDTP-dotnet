using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Preparer;

public interface IPreparer
{
    RequestBase PrepareRequest(RequestBase request);
    ResponseBase PrepareResponse(ResponseBase response);
}