using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Preparer;

public class Preparer : IPreparer
{
    public RequestBase PrepareRequest(RequestBase request)
    {
        request.Headers.Add("Prepared", "true");
        
        return request;
    }

    public ResponseBase PrepareResponse(ResponseBase response)
    {
        response.Headers.Add("Prepared", "true");
        
        return response;
    }
}