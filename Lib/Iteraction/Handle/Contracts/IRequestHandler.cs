using Lib.Iteraction.Contracts;

namespace Lib.Iteraction.Handle.Contracts;

public interface IRequestHandler
{
    Task<ResponseBase> ServeRequest(RequestBase request);
    void AddServiceContainer(IServiceProvider serviceProvider);
    void AddRoutes(Dictionary<string, Func<Context, Task>[]> routes);
}