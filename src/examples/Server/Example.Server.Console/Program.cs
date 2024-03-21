using BIDTP.Dotnet.Iteraction.Response;
using BIDTP.Dotnet.Iteraction.Response.Dtos;
using BIDTP.Dotnet.Iteraction.Response.Enums;
using BIDTP.Dotnet.Server.Builder;
using BIDTP.Dotnet.Server.Iteraction;
using BIDTP.Dotnet.Server.Options;
using Example.Server.Controllers;
using Example.Server.Providers;
using Example.Server.Repositories;
using Example.Server.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Example.Server.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ServerBuilder();

            var options = new ServerOptions("testpipe", 1024,  5000);
            builder.SetGeneralOptions(options);
            
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
            serviceCollection.AddScoped<AuthProvider>();
            serviceCollection.AddScoped<ColorProvider>();
            serviceCollection.AddScoped<ElementRepository>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            builder.AddDiContainer(serviceProvider);
             
            builder.AddRoute("PrintMessage", JustChickenGuard, MessageController.PrintMessageHandler);
            builder.AddRoute("GetElements", MessageController.GetElements);

            Task JustChickenGuard(Context context)
            {
                var request = context.Request;
        
                var isShitWord = request.Body.Contains("Yes of course");
        
                if(isShitWord)
                {
                    var dto = new Error
                    {
                        Message = "I am Alexandr Nevsky",
                        Description = "Exception: Chicken-Bodybuilder detected",
                        ErrorCode = 228
                    };
            
                    var response = new Response(StatusCode.ClientError)
                    {
                        Body = JsonConvert.SerializeObject(dto)
                    };
            
                    context.Response = response;
                }

                return Task.CompletedTask;
            }
            
            var server = builder.Build();
            
            var logger = server.Services.GetRequiredService<ILogger<BIDTP.Dotnet.Server.Server>>();
    
            logger.LogInformation("Server started");
    
            server.AddBackgroundService<LoggingWorker>("BackgroundWorker1");
            server.AddBackgroundService<LoggingWorker>("BackgroundWorker2"); 
            
            var cancellationTokenSource = new CancellationTokenSource();

            await server.StartAsync(cancellationTokenSource.Token);
        }
    }
}