using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Validator;

public class Validator : IValidator
{
    public RequestBase ValidateRequest(RequestBase request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var body = request.GetBody<object>();
        if (body is null) throw new ArgumentNullException(nameof(request));

        if (request.Headers is null) throw new ArgumentNullException(nameof(request.Headers));

        if (request.Headers["Route"] is null) throw new ArgumentNullException("Route is null");

        return request;
    }

    public ResponseBase ValidateResponse(ResponseBase response)
    {
        if (response is null) throw new ArgumentNullException(nameof(response));

        var body = response.GetBody<object>();
        if (body is null) throw new ArgumentNullException(nameof(response));

        if (response.Headers is null) throw new ArgumentNullException(nameof(response.Headers));

        return response;
    }
}