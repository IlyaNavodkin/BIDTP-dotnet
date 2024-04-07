using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BIDTP.Dotnet.Core.Extensions;
using BIDTP.Dotnet.Core.Iteraction.Builders;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Example.Server.Core.Workers;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Controllers;
using Example.Server.Domain.Colors.Providers;
using Example.Server.Domain.Elements.Repositories;
using Example.Server.Domain.Messages.Controllers;
using Example.Server.Revit.OwnerProcess.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External;
using Serilog;
using UIFrameworkServices;

namespace Example.Server.Revit.OwnerProcess.ExternalCommands;

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
        
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Text files (*.exe)|*.exe"; 
        openFileDialog.Multiselect = false;

        var result = openFileDialog.ShowDialog(); 
        if (result != true) throw new FileNotFoundException("File not found");

        var filename = openFileDialog.FileName;
        _isRunning = true;
        
        Task.Run(async () =>
        {
            Process childProcess = null;
            BIDTP.Dotnet.Core.Iteraction.Server server = null;
            
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var pipeName = AppDomain.CurrentDomain.GetHashedPipeName();
                
                var options = new ServerOptions(
                    "*", 
                    pipeName, 
                    1024,  
                    5000);
                
                var builder = new ServerBuilder();
        
                builder.SetGeneralOptions(options);
                
                var serviceCollection = new ServiceCollection();
                
                serviceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
                serviceCollection.AddScoped<AuthProvider>();
                serviceCollection.AddScoped<ColorProvider>();
                serviceCollection.AddScoped<ElementRepository>();

                var serviceProvider = serviceCollection.BuildServiceProvider();
                
                builder.AddDiContainer(serviceProvider);
                
                builder.AddRoute("PrintMessage", ShitWordGuard, ColorController.GetRandomColor);
                builder.AddRoute("GetElements", ElementRevitController.GetElements);
                builder.AddRoute("DeleteElement", ElementRevitController.DeleteElement);

                Task ShitWordGuard(Context context)
                {
                    var request = context.Request;
                    
                    var isShitWord = request
                        .GetBody<string>()
                        .Contains("Плохой");
                    
                    if(isShitWord)
                    {
                        var dto = new Error
                        {
                            Message = "Я не плохой",
                            Description = "Сам такой?",
                            ErrorCode = 228
                        };

                        var response = new Response(StatusCode.ClientError);
                        response.SetBody(dto);
                        
                        context.Response = response;
                    }

                    return Task.CompletedTask;
                }
                
                server = builder.Build();
                
                server.AddBackgroundService<LoggingWorker>();
               
                childProcess = RunClientProcess(options.PipeName, filename);
                
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
    
    private Process RunClientProcess(string pipeName, string clientPath)
    {
        var fileIsExist = File.Exists(clientPath);
        
        if (!fileIsExist) throw new FileNotFoundException($"Iteraction file not found at { clientPath }.");
        
        var processId = Process.GetCurrentProcess().Id.ToString();
    
        var arguments = "--pn=\"" + pipeName + "\" --pid=\"" + processId + "\"";
        var childProcess = Process.Start(clientPath, arguments);
        
        childProcess.Exited += (sender, args) =>
        {
            _isRunning = false;
        };
        
        return childProcess;
    }
}