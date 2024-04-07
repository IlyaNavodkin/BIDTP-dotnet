using System.Diagnostics;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Mutators.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Mutators;

public class RequestMutator : IRequestMutator
{
    public void SetGeneralHeaders(Request request)
    {
        request.Headers.Add(Constants.Constants.ProtocolHeaderName, Constants.Constants.ProtocolName);
        request.Headers.Add(Constants.Constants.ProtocolFullNameHeaderName, Constants.Constants.ProtocolFullName);
        request.Headers.Add(Constants.Constants.ProtocolVersionHeaderName, Constants.Constants.ProtocolVersion);
        request.Headers.Add(Constants.Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
    }
}