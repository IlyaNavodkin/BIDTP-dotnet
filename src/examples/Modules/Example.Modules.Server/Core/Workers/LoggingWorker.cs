using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.Modules.Server.Core.Workers;

/// <summary>
/// The logging worker - simple example of a background service
/// </summary>
public class LoggingWorker : BackgroundService
{
    private readonly ILogger<LoggingWorker> _logger;

    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="logger"> The logger. </param>
    public LoggingWorker(ILogger<LoggingWorker> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///  Background service execution
    /// </summary>
    /// <param name="stoppingToken"> The cancellation token. </param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var thread = Thread.CurrentThread;
            _logger.LogInformation("LoggingWorker is running at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("Current thread: {thread}", thread.ManagedThreadId);
            
            await Task.Delay(TimeSpan.FromSeconds(4), stoppingToken); 
        }
    }
}