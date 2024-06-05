using Lib.Iteraction.Contracts;
using Microsoft.Extensions.Logging;

namespace Lib.Iteraction.Handle.Contracts;

public interface IRequestHandler
{
    Task<ResponseBase> ServeRequest(RequestBase request);
    void SetServices(IServiceProvider serviceProvider);
    void SetRoutes(Dictionary<string, Func<Context, Task>[]> routes);
    void SetLogger(ILogger logger);
}