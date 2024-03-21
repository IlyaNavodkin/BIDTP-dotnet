using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Windows;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Iteraction;
using BIDTP.Dotnet.Iteraction.Options;
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
    public static  BIDTP.Dotnet.Iteraction.Client? Client;
    /// <summary>
    ///  The client cancel token source
    /// </summary>
    public static CancellationTokenSource ClientCancelTokenSource;

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
    protected override void OnStartup(StartupEventArgs e)
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
        
        var options = new ClientOptions("testpipe", 1024, 9000, 
            1000, 5000);
        
        Client = new BIDTP.Dotnet.Iteraction.Client(options);
        ClientCancelTokenSource = new CancellationTokenSource();
        Client.ConnectToServer(ClientCancelTokenSource);
        
        var view = new MainView();
        
        var args = Environment.GetCommandLineArgs();
        var argString = string.Join(" ", args);
        
        view.RawArgs.Text = argString;
        view.ServerName.Text = CommandLineArguments.PipeName;
        
        view.ShowDialog();
    }

    private void OnOwnerProcessExited(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(Shutdown);
    }
}