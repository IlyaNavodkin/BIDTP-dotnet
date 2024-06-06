// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Bytes;
using BIDTP.Dotnet.Core.Iteraction.Validation;
using BIDTP.Dotnet.Core.Iteraction.Mutation;
using BIDTP.Dotnet.Core.Iteraction.Logger;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Bytes;
using BIDTP.Dotnet.Core.Iteraction.Serialization;
using BIDTP.Dotnet.Core.Iteraction;
using Schemas;

async Task Main()
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    var tasks = new List<Task>();

    for (int i = 0; i <1; i++)
    {
        tasks.Add(CreateAndSendFromConsole());
    }

    await Task.WhenAll(tasks);

    //for (int i = 0; i < 5000; i++)
    //{
    //    await CreateAndSend();
    //}

    stopwatch.Stop();

    Console.WriteLine($"Total time taken: {stopwatch.ElapsedMilliseconds} ms");

    Console.ReadKey();
}

static async Task CreateAndSend()
{
    var btw = new ByteWriter();
    var btr = new ByteReader();
    var ser = new Serializer(Encoding.Unicode);
    var val = new Validator();
    var prep = new Preparer();

    var client = new BidtpClient();

    client.SetPipeName("testPipe");

    var body = new Computer
    {
        Id = 1,
        Name = Guid.NewGuid().ToString(),
        Components = new List<Component>
        {
            new Component
            {
                Id = 1,
                Name = "Rtx 3060"
            }
        }
    };

    var request = new Request
    {
        Headers = new Dictionary<string, string>
        {
            ["Route"] = "check"
        },
        Body = JsonSerializer.Serialize(body)
    };

    var response = await client.Send(request);
    var responseBody = response.GetBody<string>();

    Console.WriteLine(responseBody);
}

static async Task CreateAndSendFromConsole()
{
    var btw = new ByteWriter();
    var btr = new ByteReader();
    var ser = new Serializer(Encoding.Unicode);
    var val = new Validator();
    var prep = new Preparer();

    var client = new BidtpClient();

    client.SetPipeName("testPipe");

    while (true) // Бесконечный цикл
    {
        Console.WriteLine("Enter the route (or 'exit' to quit):");
        var route = Console.ReadLine(); // Считываем ввод с консоли

        var body = new Computer
        {
            Id = 1,
            Name = Guid.NewGuid().ToString(),
            Components = new List<Component>
            {
                new Component
                {
                    Id = 1,
                    Name = "Rtx 3060"
                }
            }
        };

        var request = new Request
        {
            Headers = new Dictionary<string, string>
            {
                ["Route"] = route // Устанавливаем значение маршрута из ввода с консоли
            },
            Body = JsonSerializer.Serialize(body)
        };

        var response = await client.Send(request);
        var responseBody = response.GetBody<string>();

        Console.WriteLine(responseBody);
    }
}

await Main();
