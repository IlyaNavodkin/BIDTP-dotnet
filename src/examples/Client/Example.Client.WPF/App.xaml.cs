using System.Diagnostics;
using System;
using System.Threading;
using System.Windows;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Core.Iteraction;
using Example.Client.WPF.Views;
using Example.Client.WPF.Args;

namespace Example.Client.WPF;

/// <summary>
///     Programme entry point
/// </summary>
public sealed partial class App
{
    private static readonly CommandLineArguments? CommandLineArguments;

    /// <summary>
    ///  The client 
    /// </summary>
    public static BidtpClient Client;

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

        if (!pidParse)
        {
            CommandLineArguments.PipeName = "testpipe";
            MessageBox.Show($"Process id is not valid or its value is null." +
                $" Set default pipe name {CommandLineArguments.PipeName}");
        }
        else
        {
            OwnerProcess = Process.GetProcessById(pid);

            OwnerProcess.EnableRaisingEvents = true;
            OwnerProcess.Exited += OnOwnerProcessExited;
        }

        Client = new BidtpClient();

        Client.Pipename = CommandLineArguments.PipeName;

        var view = new MainWindow();

        view.ShowDialog();
    }

    private void OnOwnerProcessExited(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(Shutdown);
    }
}