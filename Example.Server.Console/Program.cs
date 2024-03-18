﻿using System.Text.Encodings.Web;
using System.Text.Json;
using Example.Server.Controllers;
using Example.Server.Providers;
using Example.Server.Repositories;
using Example.Server.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piders.Dotnet.Core.Response;
using Piders.Dotnet.Core.Response.Dtos;
using Piders.Dotnet.Core.Response.Enums;
using Piders.Dotnet.Server.Builder;
using Piders.Dotnet.Server.Providers;
using Piders.Dotnet.Server.Server;
using Piders.Dotnet.Server.Server.Iteraction;

namespace TestServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ServerBuilder();

            var options = new ServerOptions("testpipe", 1024,  5000);
            builder.SetGeneralOptions(options);
            
            builder.ServiceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
            builder.ServiceCollection.AddScoped<AuthProvider>();
            builder.ServiceCollection.AddScoped<ColorProvider>();
            builder.ServiceCollection.AddScoped<ElementRepository>();
            
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
            
            var logger = server.Services.GetRequiredService<ILogger<Server>>();
    
            logger.LogInformation("Server started");
    
            server.AddBackgroundService<LoggingWorker>("BackgroundWorker1");
            server.AddBackgroundService<LoggingWorker>("BackgroundWorker2"); 
            
            var cancellationTokenSource = new CancellationTokenSource();

            await server.StartAsync(cancellationTokenSource.Token);
        }
    }
}