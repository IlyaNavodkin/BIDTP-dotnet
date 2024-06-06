using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using System;

namespace BIDTP.Dotnet.Core.Iteraction.Validation;

public class Validator : IValidator
{
    public RequestBase ValidateRequest(RequestBase request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var body = request.GetBody<string>();
        if (body is null) throw new ArgumentNullException(nameof(request));

        if (request.Headers is null) throw new ArgumentNullException(nameof(request.Headers));

        if (request.Headers["Route"] is null) throw new ArgumentNullException("Route is null");

        return request;
    }

    public ResponseBase ValidateResponse(ResponseBase response)
    {
        if (response is null) throw new ArgumentNullException(nameof(response));

        var body = response.GetBody<string>();
        if (body is null) throw new ArgumentNullException(nameof(response));

        if (response.Headers is null) throw new ArgumentNullException(nameof(response.Headers));

        return response;
    }
}