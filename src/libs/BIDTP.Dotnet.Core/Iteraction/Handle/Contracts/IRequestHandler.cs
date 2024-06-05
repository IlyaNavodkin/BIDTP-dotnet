using BIDTP.Dotnet.Core.Iteraction.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;

public interface IRequestHandler
{
    Task<ResponseBase> ServeRequest(RequestBase request);
}