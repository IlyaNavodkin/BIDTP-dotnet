using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Logger;
using BIDTP.Dotnet.Core.Iteraction.Schema;
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
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Example.Server.Console
{
    public class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _logger;

        public SerilogLogger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console() 
                .CreateLogger();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Serilog не поддерживает области логирования, поэтому возвращаем пустой IDisposable
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Преобразование уровня логирования из Microsoft.Extensions.Logging в Serilog
            return _logger.IsEnabled(ConvertLogLevel(logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Преобразование уровня логирования из Microsoft.Extensions.Logging в Serilog
            var serilogLogLevel = ConvertLogLevel(logLevel);

            // Вызываем Serilog для записи лога
            _logger.Write(serilogLogLevel, exception, formatter(state, exception));
        }

        // Преобразование уровня логирования из Microsoft.Extensions.Logging в Serilog
        private Serilog.Events.LogEventLevel ConvertLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
                LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
                LogLevel.Information => Serilog.Events.LogEventLevel.Information,
                LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
                LogLevel.Error => Serilog.Events.LogEventLevel.Error,
                LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Information, // По умолчанию используется Information
            };
        }
    }

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new BidtpServerBuilder();

            builder.Services.AddHostedService<LoggingWorker>();

            
            //builder.Services.AddSingleton<ILogger, ConsoleLogger>();
            //builder.Services.AddSingleton<ILogger, SerilogLogger>();
            builder.Services.AddTransient<AuthProvider>();
            builder.Services.AddTransient<ColorProvider>();
            builder.Services.AddTransient<ElementRepository>();
            builder.Services.AddLogging(
                builder =>
                {
                    builder
                        .ClearProviders()
                        .AddSerilog();
                });

            builder.WithPipeName("testpipe");
            builder.WithProcessPipeQueueDelayTime(100);
             
            builder.AddRoute("PrintMessage", JustChickenGuard, ColorController.GetRandomColor);
            builder.AddRoute("GetElements", ElementController.GetElements);
            builder.AddRoute("MutateUrTable", DataTableController.MutateUrTable);
            builder.AddRoute("GetMappedObjectFromObjectContainer", ObjectContainerMiddleware.Handle,
                SendMessageController.GetMappedObjectWithMetadataFromObjectContainer);
            
            Task JustChickenGuard(Context context)
            {
                var request = context.Request;
        
                Task.Delay(2000).GetAwaiter().GetResult();

                var logger = context.ServiceProvider.GetRequiredService<ILogger>();

                var thread = Thread.CurrentThread;

                logger.LogWarning($"Current Thread: {thread.ManagedThreadId}");

                var isShitWord = request
                    .GetBody<string>()
                    .Contains("Yes of course");
        
                if(isShitWord)
                {
                    var dto = new BIDTPError
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
            
            var logger = server.Services.GetRequiredService<ILogger>();
    
            logger.LogInformation("Server started");
                
            var cancellationTokenSource = new CancellationTokenSource();

            await server.Start(cancellationTokenSource.Token);
        }
    }
}