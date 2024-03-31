using BIDTP.Dotnet.Core.Iteraction.Builders;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Example.Server.Core.Workers;
using Example.Server.Domain.Auth.Middlewares;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Controllers;
using Example.Server.Domain.Colors.Providers;
using Example.Server.Domain.DataTable;
using Example.Server.Domain.Elements.Controllers;
using Example.Server.Domain.Elements.Repositories;
using Example.Server.Domain.Messages.Controllers;
using Example.Server.Domain.Messages.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Example.Server.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ServerBuilder();

            var options = new ServerOptions("*","testpipe", 1024,  5000);
            builder.SetGeneralOptions(options);
            
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
            serviceCollection.AddTransient<AuthProvider>();
            serviceCollection.AddTransient<ColorProvider>();
            serviceCollection.AddTransient<ElementRepository>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            builder.AddDiContainer(serviceProvider);
             
            builder.AddRoute("PrintMessage", JustChickenGuard, ColorController.GetRandomColor);
            builder.AddRoute("GetElements", ElementController.GetElements);
            builder.AddRoute("MutateUrTable", DataTableController.MutateUrTable);
            builder.AddRoute("GetMappedObjectFromObjectContainer", ObjectContainerMiddleware.Handle,
                SendMessageController.GetMappedObjectWithMetadataFromObjectContainer);
            
            Task JustChickenGuard(Context context)
            {
                var request = context.Request;
        
                var isShitWord = request
                    .GetBody<string>()
                    .Contains("Yes of course");
        
                if(isShitWord)
                {
                    var dto = new Error
                    {
                        Message = "I am Alexandr Nevsky",
                        Description = "Exception: Chicken-Bodybuilder detected",
                        ErrorCode = 228
                    };

                    var response = new Response(StatusCode.ClientError);

                    response.SetBody(dto);
            
                    context.Response = response;
                }

                return Task.CompletedTask;
            }
            
            var server = builder.Build();
            
            var logger = server.Services.GetRequiredService<ILogger<BIDTP.Dotnet.Core.Iteraction.Server>>();
    
            logger.LogInformation("Server started");
    
            server.AddBackgroundService<LoggingWorker>("BackgroundWorker1");
            server.AddBackgroundService<LoggingWorker>("BackgroundWorker2"); 
            
            var cancellationTokenSource = new CancellationTokenSource();

            await server.StartAsync(cancellationTokenSource.Token);
        }
    }
}