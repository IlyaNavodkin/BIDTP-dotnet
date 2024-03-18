using Piders.Dotnet.Server.Server;
using Piders.Dotnet.Server.Server.Iteraction;

namespace Piders.Dotnet.Server.Handlers;

public interface IRequestHandler
{
    Task HandleRequestAsync(Context context);
}