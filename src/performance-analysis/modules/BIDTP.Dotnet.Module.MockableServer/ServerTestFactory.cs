using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Module.MockableServer.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Module.MockableServer;

public static class ServerTestFactory
{
    public static BidtpServer CreateServer()
    {
        var builder = new BidtpServerBuilder();

        builder.WithController<SendMessageController>();
       
        var server = builder.Build();

        var logger = server.Services.GetRequiredService<ILogger>();

        logger.LogInformation("Server test factory created server");
        
        return server;
    }
}