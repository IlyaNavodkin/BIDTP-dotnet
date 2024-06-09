using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Schema;
using Example.Server.Core.Workers;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Controllers;
using Example.Server.Domain.Colors.Providers;
using Example.Server.Domain.Elements.Controllers;
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

        //var result = openFileDialog.ShowDialog(); 
        //if (result != true) throw new FileNotFoundException("File not found");

        var filename = openFileDialog.FileName;
        
        Task.Run(async () =>
        {
            Process childProcess = null;
            BidtpServer server = null;
            
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                
                var builder = new BidtpServerBuilder();
                
                builder.Services.AddScoped<AuthProvider>();
                builder.Services.AddScoped<ColorProvider>();
                builder.Services.AddScoped<ElementRepository>();

                builder.WithPipeName("testpipe");
                builder.WithProcessPipeQueueDelayTime(100);

                builder.WithController<ColorController>();
                builder.WithController<ElementRevitController>();
                
                server = builder.Build();
                               
                //childProcess = RunClientProcess(server.PipeName, filename);
                

                MessageBox.Show("Server started");
                await server.Start(cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                if (childProcess != null) childProcess.Kill();
            }
            finally
            {
                if (server != null) server.Stop();
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