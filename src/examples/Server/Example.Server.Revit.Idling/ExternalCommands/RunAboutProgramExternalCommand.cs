using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Example.Server.Revit;
using Microsoft.Win32;

namespace Example.Modules.Server.Revit.ExternalCommands;

/// <summary>
///  The run about program external command
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class RunAboutProgramExternalCommand : Nice3point.Revit.Toolkit.External.ExternalCommand
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
            _isRunning = false;
        });

        if (_isRunning)
        {
            MessageBox.Show($"UI process of {nameof(RunAboutProgramExternalCommand)} is already running");

            Result = Result.Cancelled;
            return;
        }

        var openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Text files (*.exe)|*.exe";
        openFileDialog.Multiselect = false;

        var result = openFileDialog.ShowDialog();
        if (result != true) throw new FileNotFoundException("File not found");

        var filename = openFileDialog.FileName;

        var server = SimpleDimpleExternalApplication.BidtpServer;

        if (server == null) throw new NullReferenceException("BidtpServer is null");

        var childProcess = RunClientProcess(server.PipeName, filename);

        Result = Result.Succeeded;
    }

    private Process RunClientProcess(string pipeName, string clientPath)
    {
        var fileIsExist = File.Exists(clientPath);

        if (!fileIsExist) throw new FileNotFoundException($"Iteraction file not found at {clientPath}.");

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