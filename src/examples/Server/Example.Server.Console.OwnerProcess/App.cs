using System.Diagnostics;
using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using Example.Server.Core.Utils;
using Example.Server.Core.Workers;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Controllers;
using Example.Server.Domain.Colors.Providers;
using Example.Server.Domain.Elements.Controllers;
using Example.Server.Domain.Elements.Repositories;
using Example.Server.Domain.Messages.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var cancellationTokenSource = new CancellationTokenSource();
Process childProcess = null;

try
{
    var builder = new BidtpServerBuilder();

    var serverName = "testpipe";

    builder.Services.AddHostedService<LoggingWorker>();

    builder.Services.AddTransient<AuthProvider>();
    builder.Services.AddTransient<ColorProvider>();
    builder.Services.AddTransient<ElementRepository>();

    builder.WithPipeName(serverName);
    builder.WithProcessPipeQueueDelayTime(100);

    builder.WithController<ColorController>();
    builder.WithController<SendMessageController>();

    var server = builder.Build();

    var logger = server.Services.GetRequiredService<ILogger>();

    logger.LogInformation("Server started");

    childProcess = RunClientProcess(serverName);

    await server.Start(cancellationTokenSource.Token);
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    Console.ReadKey();

    if (childProcess != null) childProcess.Kill();

    Environment.Exit(1);
}

Process RunClientProcess(string pipeName)
{
    var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
    var parentDirectory = currentDirectory.Parent.Parent.Parent;

    var fileName = SearchFile(parentDirectory.FullName, "Example.Client.WPF.exe");

    if (string.IsNullOrEmpty(fileName))
    {
        throw new FileNotFoundException("File Example.Client.WPF.exe not found in parent directory. " +
            "Build Example.Client.WPF project first then copy it to parent directory.");
    }

    var processId = Process.GetCurrentProcess().Id.ToString();

    var arguments = $"--pn=\"{pipeName}\" --pid=\"{processId}\"";
    var childProcess = Process.Start(fileName, arguments);

    return childProcess;
}

string SearchFile(string rootDirectory, string fileName)
{
    foreach (var file in Directory.GetFiles(rootDirectory, fileName, SearchOption.AllDirectories))
    {
        return file;
    }
    return null;
}
