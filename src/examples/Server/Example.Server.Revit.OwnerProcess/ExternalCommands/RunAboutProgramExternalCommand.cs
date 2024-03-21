using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BIDTP.Dotnet.Extensions;
using BIDTP.Dotnet.Iteraction.Builders;
using BIDTP.Dotnet.Iteraction.Dtos;
using BIDTP.Dotnet.Iteraction.Enums;
using BIDTP.Dotnet.Iteraction.Options;
using BIDTP.Dotnet.Iteraction.Providers;
using Example.Server.Controllers;
using Example.Server.Providers;
using Example.Server.Repositories;
using Example.Server.Revit.Controllers;
using Example.Server.Utils;
using Example.Server.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nice3point.Revit.Toolkit.External;
using Serilog;

namespace Example.Server.Revit.ExternalCommands;

/// <summary>
///  The run about program external command
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class RunAboutProgramExternalCommand: ExternalCommand
{
    /// <summary>
    ///  Is running flag
    /// </summary>
    private static bool _isRunning;
    /// <summary>
    ///  Execute the external command
    /// </summary>
    public override void Execute()
    {
        
        SuppressExceptions(exception =>
        {
            Log.Fatal(exception, $"Unhandled exception in { nameof(RunAboutProgramExternalCommand) }");
        });

        if (_isRunning)
        {
            MessageBox.Show($"UI process of { nameof(RunAboutProgramExternalCommand) } is already running");
            
            Result = Result.Cancelled;
            return;
        }
        
        _isRunning = true;
        
        _ = Task.Run(async () =>
        {
            Process childProcess = null;
            BIDTP.Dotnet.Iteraction.Server server = null;
            
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var pipeName = AppDomain.CurrentDomain.GetHashedPipeName();
                
                var options = new ServerOptions(pipeName, 1024,  5000);
                var builder = new ServerBuilder();
        
                builder.SetGeneralOptions(options);
                
                var serviceCollection = new ServiceCollection();
                
                serviceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
                serviceCollection.AddScoped<AuthProvider>();
                serviceCollection.AddScoped<ColorProvider>();
                serviceCollection.AddScoped<ElementRepository>();

                var serviceProvider = serviceCollection.BuildServiceProvider();
                
                builder.AddDiContainer(serviceProvider);
                
                builder.AddRoute("PrintMessage", ShitWordGuard, MessageController.PrintMessageHandler);
                builder.AddRoute("GetElements", ElementRevitController.GetElements);
                builder.AddRoute("DeleteElement", ElementRevitController.DeleteElement);

                Task ShitWordGuard(Context context)
                {
                    var request = context.Request;
                    
                    var isShitWord = request.Body.Contains("Плохой");
                    
                    if(isShitWord)
                    {
                        var dto = new Error
                        {
                            Message = "Я не плохой",
                            Description = "Сам такой?",
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
                
                server = builder.Build();
                
                server.AddBackgroundService<LoggingWorker>();
               
                childProcess = RunClientProcess(options.PipeName);
                
                await server.StartAsync(cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                if (childProcess != null) childProcess.Kill();
            }
            finally
            {
                if (server != null) await server.StopAsync();
                _isRunning = false;
            }
        });
        
        Result = Result.Succeeded;
    }
    
    private Process RunClientProcess(string pipeName)
    {
        var currentDirectory = new DirectoryInfo (Directory.GetCurrentDirectory());
        var parentDirectory = currentDirectory.Parent.Parent.Parent;
    
        var fileName = FileUtils.SearchFile(parentDirectory.FullName, "Example.Client.WPF.ChildProcess.exe");
    
        var processId = Process.GetCurrentProcess().Id.ToString();
    
        var arguments = "--pn=\"" + pipeName + "\" --pid=\"" + processId + "\"";
        var childProcess = Process.Start(fileName, arguments);
        
        return childProcess;
    }
}