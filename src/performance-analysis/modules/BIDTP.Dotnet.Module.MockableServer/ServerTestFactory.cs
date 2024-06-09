using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using Example.Server.Domain.Auth.Middlewares;
using Example.Server.Domain.Messages.Controllers;
using Example.Server.Domain.Messages.Middlewares;
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