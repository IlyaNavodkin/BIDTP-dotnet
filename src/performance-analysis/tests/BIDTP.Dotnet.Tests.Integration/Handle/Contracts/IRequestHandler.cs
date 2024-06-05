using Lib.Iteraction.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;

namespace Lib.Iteraction.Handle.Contracts;

public interface IRequestHandler
{
    Task<ResponseBase> ServeRequest(RequestBase request);
}