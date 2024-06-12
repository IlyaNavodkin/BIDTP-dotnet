using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using System;
using System.Diagnostics;

namespace BIDTP.Dotnet.Core.Iteraction.Mutation;

public class Preparer : IPreparer
{
    public RequestBase PrepareRequest(RequestBase request)
    {
        request.Headers.Add(Constants.ProtocolHeaderName, Constants.ProtocolName);
        request.Headers.Add(Constants.ProtocolFullNameHeaderName, Constants.ProtocolFullName);
        request.Headers.Add(Constants.ProtocolVersionHeaderName, Constants.ProtocolVersion);
        request.Headers.Add(Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
        request.Headers.Add(Constants.RequestResponseGuidHeaderName, Guid.NewGuid().ToString());

        return request;
    }

    public ResponseBase PrepareResponse(ResponseBase response)
    {
        response.Headers.Add(Constants.ProtocolHeaderName, Constants.ProtocolName);
        response.Headers.Add(Constants.ProtocolFullNameHeaderName, Constants.ProtocolFullName);
        response.Headers.Add(Constants.ProtocolVersionHeaderName, Constants.ProtocolVersion);
        response.Headers.Add(Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
        response.Headers.Add(Constants.RequestResponseGuidHeaderName, Guid.NewGuid().ToString());

        return response;
    }
}