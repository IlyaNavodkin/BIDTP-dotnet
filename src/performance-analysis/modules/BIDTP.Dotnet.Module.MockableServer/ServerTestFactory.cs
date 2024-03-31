using BIDTP.Dotnet.Core.Iteraction.Builders;
using BIDTP.Dotnet.Core.Iteraction.Options;
using Example.Server.Domain.Auth.Middlewares;
using Example.Server.Domain.Messages.Controllers;
using Example.Server.Domain.Messages.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Module.MockableServer;

public static class ServerTestFactory
{
    public static Core.Iteraction.Server CreateServer()
    {
        var builder = new ServerBuilder();

        var options = new ServerOptions("testpipe", 1024,  5000);
        builder.SetGeneralOptions(options);

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));

        var serviceProvider = serviceCollection.BuildServiceProvider();

        builder.AddDiContainer(serviceProvider);

        builder.AddRoute("GetMessageForAdmin", AuthMiddleware.Handle, SendMessageController.GetMessageForAdmin);
        builder.AddRoute("GetMessageForUser", AuthMiddleware.Handle, SendMessageController.GetMessageForUser);
        builder.AddRoute("GetAuthAccessResponse", AuthMiddleware.Handle,SendMessageController.GetAuthAccessResponse);
        builder.AddRoute("GetFreeAccessResponse", SendMessageController.GetFreeAccessResponse);
        builder.AddRoute("GetMappedObjectFromObjectContainer", ObjectContainerMiddleware.Handle, 
            SendMessageController.GetMappedObjectWithMetadataFromObjectContainer);
        
        var server = builder.Build();

        var logger = server.Services.GetRequiredService<ILogger<Core.Iteraction.Server>>();

        logger.LogInformation("Server started");
        
        return server;
    }
}