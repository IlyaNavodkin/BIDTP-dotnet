using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Build;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schemas;

var builder = new BidtpServerBuilder();

var serviceContainer = new ServiceCollection();

builder.WithPipeName("testPipe");
builder.WithProcessPipeQueueDelayTime(100);

builder.AddRoute("check", JustChickenGuard);
builder.AddRoute("m", GetRequestFullInfo);

var server = builder.Build();

var cancelTokenSource = new CancellationTokenSource();

await server.Start(cancelTokenSource.Token);

Console.ReadKey();

Task JustChickenGuard(Context context)
{
    var request = context.Request;

    var result = new Result
    {
        Data = new List<Component>
        {
            new Component { Id = 1, Name = "Rtx 4070" },
            new Component { Id = 2, Name = "Rtx 4080" },
        },
    };

    var response = new Response(StatusCode.Success)
    {
        Body = JsonSerializer.Serialize(result)
    };

    context.Response = response;

    return Task.CompletedTask;
}

Task GetRequestFullInfo(Context context)
{
    var server = context.ServiceProvider;

    var logger = server.GetRequiredService<ILogger>();

    logger.LogInformation("Off called");

    var request = context.Request;

    var jsonHeaders = JsonSerializer.Serialize(request.Headers, new JsonSerializerOptions { WriteIndented = true });
    var jsonBody = JsonSerializer.Serialize(request.Body, new JsonSerializerOptions { WriteIndented = true });

    logger.LogInformation("Headers: {Headers}", jsonHeaders);
    logger.LogInformation("Body: {Body}", jsonBody);

    var response = new Response(StatusCode.Success)
    {
        Body = "Hello World"
    };

    context.Response = response;

    return Task.CompletedTask;
}

