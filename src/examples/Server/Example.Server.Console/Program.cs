using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Events;
using BIDTP.Dotnet.Core.Iteraction.Schema;
using Example.Server.Core.Workers;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Controllers;
using Example.Server.Domain.Colors.Providers;
using Example.Server.Domain.Elements.Controllers;
using Example.Server.Domain.Elements.Repositories;
using Example.Server.Domain.Messages.Controllers;
using Example.Server.Domain.Messages.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Example.Server.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new BidtpServerBuilder();

            builder.Services.AddHostedService<LoggingWorker>();

            builder.Services.AddTransient<AuthProvider>();
            builder.Services.AddTransient<ColorProvider>();
            builder.Services.AddScoped<ElementRepository>();

            builder.WithPipeName("testpipe");
            builder.WithProcessPipeQueueDelayTime(100);

            builder.WithController<ColorController>();
            builder.WithController<AppleController>();
            builder.WithController<ElementsController>();
            builder.WithController<BookController>();
             
            var server = builder.Build();

            server.RequestReceived += (s, e) =>
            {
                var eventArgs = (RequestReceivedProgressEventArgs)e;

                var logger = server.Services.GetRequiredService<ILogger>();

                logger.LogInformation ($"Request received");
            };

            server.ResponseSended += (s, e) =>
            {
                var eventArgs = (ResponseSendedProgressEventArgs)e;

                var logger = server.Services.GetRequiredService<ILogger>();

                logger.LogInformation($"Request sended");
            };

            var logger = server.Services.GetRequiredService<ILogger>();
    
            logger.LogInformation("Server started");
                
            var cancellationTokenSource = new CancellationTokenSource();

            await server.Start(cancellationTokenSource.Token);
        }
    }
}