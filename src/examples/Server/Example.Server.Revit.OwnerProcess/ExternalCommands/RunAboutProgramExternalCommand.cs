using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BIDTP.Dotnet.Core.Response;
using BIDTP.Dotnet.Core.Response.Dtos;
using BIDTP.Dotnet.Core.Response.Enums;
using BIDTP.Dotnet.Server.Builder;
using BIDTP.Dotnet.Server.Providers;
using BIDTP.Dotnet.Server.Server;
using BIDTP.Dotnet.Server.Server.Iteraction;
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
///  Команда "О программе проекта"
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class RunAboutProgramExternalCommand: ExternalCommand
{
    /// <summary>
    ///  Флаг запуска
    /// </summary>
    private static bool _isRunning;
    /// <summary>
    ///  Запуск окна "О программе проекта"
    /// </summary>
    public override void Execute()
    {
        
        SuppressExceptions(exception =>
        {
            _isRunning = false;
            Log.Fatal(exception, "Unhandled exception");
        });

        if (_isRunning)
        {
            MessageBox.Show("Программа уже запущена");
            
            Result = Result.Cancelled;
            return;
        }
        
        _isRunning = true;
        
        _ = Task.Run(async () =>
        {
            Process childProcess = null;
            BIDTP.Dotnet.Server.Server.Server server = null;
            
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var options = new ServerOptions("testpipe", 1024,  5000);
                var builder = new ServerBuilder();
        
                builder.SetGeneralOptions(options);
                
                builder.ServiceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
                builder.ServiceCollection.AddScoped<AuthProvider>();
                builder.ServiceCollection.AddScoped<ColorProvider>();
                builder.ServiceCollection.AddScoped<ElementRepository>();
                
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
    
    Process RunClientProcess(string pipeName)
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