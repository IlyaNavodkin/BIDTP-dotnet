﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Windows;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Core.Iteraction.Options;
using Example.Client.WPF.ChildProcess.Args;
using Example.Client.WPF.ChildProcess.Views;

namespace Example.Client.WPF.ChildProcess;

/// <summary>
///     Programme entry point
/// </summary>
public sealed partial class App
{
    private static readonly CommandLineArguments? CommandLineArguments;
    
    /// <summary>
    ///  The client 
    /// </summary>
    public static  BIDTP.Dotnet.Core.Iteraction.Client? Client;

    static App()
    {
        Debug.WriteLine("Starting client");
        
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        var args = Environment.GetCommandLineArgs();
        
        CommandLineArguments = ArgumentParseUtil.Parse(args);
    }
    /// <summary>
    ///  The owner process
    /// </summary>
    private static Process? OwnerProcess { get; set; }
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.ExceptionObject.ToString());
        
        Environment.Exit(1);
    }

    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="e"> The <see cref="StartupEventArgs"/> instance containing the event data.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        if (CommandLineArguments is null) throw new ArgumentNullException(nameof(CommandLineArguments));
            
        var pidParse = int.TryParse(CommandLineArguments.OwnerProcessId, out var pid);
        
        if(!pidParse)
        {
            MessageBox.Show("Invalid process id");
            Environment.Exit(1);
        }
        
        OwnerProcess = Process.GetProcessById(pid);
        
        OwnerProcess.EnableRaisingEvents = true;
        OwnerProcess.Exited += OnOwnerProcessExited;
        
        var options = new ClientOptions(CommandLineArguments.PipeName, 1024, 9000, 
            1000, 5000);
        
        Client = new BIDTP.Dotnet.Core.Iteraction.Client(options);
        
        var view = new MainWindow();
        
        view.ShowDialog();
    }

    private void OnOwnerProcessExited(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(Shutdown);
    }
}