using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.RequestServer;

public interface IRequestServer
{
    Task<ResponseBase> ServeRequest(RequestBase request);
}