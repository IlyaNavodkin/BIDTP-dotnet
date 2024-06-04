using Lib.Iteraction.Contracts;

namespace Lib.Iteraction.Handle.Contracts;

public interface IRequestHandler
{
    Task<ResponseBase> ServeRequest(RequestBase request);
}