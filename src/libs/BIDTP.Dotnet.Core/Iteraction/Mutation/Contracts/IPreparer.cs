using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;

public interface IPreparer
{
    RequestBase PrepareRequest(RequestBase request);
    ResponseBase PrepareResponse(ResponseBase response);
}