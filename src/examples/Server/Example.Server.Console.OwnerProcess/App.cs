using System.Diagnostics;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Builders;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Example.Server.Controllers;
using Example.Server.Providers;
using Example.Server.Repositories;
using Example.Server.Utils;
using Example.Server.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

var cancellationTokenSource = new CancellationTokenSource();
Process childProcess = null;

try
{
    var options = new ServerOptions("testpipe", 1024,  5000);
    var builder = new ServerBuilder();

    builder.SetGeneralOptions(options);
            
    var serviceCollection = new ServiceCollection();
    var serviceProvider = serviceCollection.BuildServiceProvider();
    
    serviceCollection.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Information));
    serviceCollection.AddScoped<AuthProvider>();
    serviceCollection.AddScoped<ColorProvider>();
    serviceCollection.AddScoped<ElementRepository>();
    
    builder.AddDiContainer(serviceProvider);
    
    builder.AddRoute("PrintMessage", ShitWordGuard, MessageController.PrintMessageHandler);
    builder.AddRoute("GetElements", MessageController.GetElements);

    Task ShitWordGuard(Context context)
    {
        var request = context.Request;
        
        var isShitWord = request.Body.Contains("Дурак");

        if (!isShitWord) return Task.CompletedTask;
        
        var dto = new Error
        {
            Message = "Сам такой",
            Description = "Зачем материшься",
            ErrorCode = 228
        };
            
        var response = new Response(StatusCode.ClientError)
        {
            Body = JsonConvert.SerializeObject(dto)
        };
            
        context.Response = response;

        return Task.CompletedTask;
    }

    var server = builder.Build();
            
    var logger = server.Services.GetRequiredService<ILogger<Server>>();
    
    logger.LogInformation("Server started");
    
    server.AddBackgroundService<LoggingWorker>("BackgroundWorker1");
    server.AddBackgroundService<LoggingWorker>("BackgroundWorker2"); 
    
    var pipeName = server.GetPipeName();
    
    childProcess = RunClientProcess(pipeName);

    await server.StartAsync(cancellationTokenSource.Token);
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    Console.ReadKey();
    if(childProcess != null) childProcess.Kill();
    Environment.Exit(1);
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

