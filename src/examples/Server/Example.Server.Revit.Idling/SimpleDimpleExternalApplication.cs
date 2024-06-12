using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;
using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using Example.Modules.Revit.Controllers;
using Example.Modules.Server.Domain.Auth.Providers;
using Example.Modules.Server.Domain.Colors.Providers;
using Example.Server.Revit.Configurator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Toolkit.External.Handlers;
using Exception = System.Exception;

namespace Example.Server.Revit;

/// <summary>
///  The simple dimple external application
/// </summary>
/// <remarks>The application should be placed in *.addin.</remarks>
public class SimpleDimpleExternalApplication: ExternalApplication
{
    /// <summary>
    ///  The async event handler
    /// </summary>
    public static AsyncEventHandler AsyncEventHandler { get; set; }

    public static BidtpServer BidtpServer;
    /// <inheritdoc/>
    public override void OnStartup()
    {
        try
        {            
            AsyncEventHandler = new AsyncEventHandler();

            var configurator = RevitUiConfigurator.GetInstance();
            configurator.ConfigureRevitUiComponents(Application);

            Task.Run(StartBidtpServer);
        }
        catch (Exception exception)
        {
            Result = Result.Failed;
        }
    }

    private async Task StartBidtpServer()
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var builder = new BidtpServerBuilder();

            builder.Services.AddSingleton<AsyncEventHandler>(AsyncEventHandler);

            builder.Services.AddScoped<AuthProvider>();
            builder.Services.AddScoped<ColorProvider>();

            builder.WithPipeName("testpipe");
            builder.WithProcessPipeQueueDelayTime(100);

            builder.WithController<RevitColorController>();
            builder.WithController<ElementRevitController>();

            BidtpServer = builder.Build();

            var logger = BidtpServer.Services.GetService<ILogger>();

            var threadId = Thread.CurrentThread.ManagedThreadId;
            var pid = Process.GetCurrentProcess().Id;

            logger.LogInformation("Process id: {pid}", pid);
            logger.LogInformation("Thread id: {threadId}", threadId);

            MessageBox.Show($"Server started with pid: {pid} and thread id: {threadId}");

            await BidtpServer.Start(cancellationTokenSource.Token);
        }
        catch (Exception exception) 
        {
            MessageBox.Show($"Server failed with exception: {exception.Message}");
        }
        finally
        {
            BidtpServer?.Dispose();
            BidtpServer = null;
        }
    }
}