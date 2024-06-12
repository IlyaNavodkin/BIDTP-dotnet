using System.Diagnostics;
using BIDTP.Dotnet.Core.Build;
using Example.Modules.Server.Domain.Elements.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Example.Modules.Server.Domain.Colors.Controllers;
using Example.Modules.Server.Domain.Auth.Providers;
using Example.Modules.Server.Core.Workers;
using Example.Modules.Server.Domain.Colors.Providers;
using Example.Modules.Server.Domain.Apple.Controllers;
using Example.Modules.Server.Domain.Elements.Controllers;
using Example.Modules.Server.Domain.Books.Controllers;

var cancellationTokenSource = new CancellationTokenSource();
Process childProcess = null;

try
{
    var builder = new BidtpServerBuilder();

    var serverName = "testpipe";

    builder.Services.AddHostedService<LoggingWorker>();

    builder.Services.AddTransient<AuthProvider>();
    builder.Services.AddTransient<ColorProvider>();
    builder.Services.AddScoped<ElementRepository>();

    builder.WithPipeName("testpipe");
    builder.WithProcessPipeQueueDelayTime(100);

    builder.WithController<ColorController>();
    builder.WithController<AppleController>();
    builder.WithController<ElementsController>();
    builder.WithController<BookController>();

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
