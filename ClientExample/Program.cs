// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lib.Iteraction;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.Request;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;
using Schemas;

async Task Main()
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    var tasks = new List<Task>();

    for (int i = 0; i <1; i++)
    {
        tasks.Add(CreateAndSend());
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

    var client = new ClientBase(val, prep, ser, btw, btr);

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
            ["Route"] = "getNewComponents"
        },
        Body = JsonSerializer.Serialize(body)
    };

    var response = await client.Send(request);
    var responseBody = response.GetBody<string>();

    Console.WriteLine(responseBody);


    //var body2 = new Computer
    //{
    //    Id = 2,
    //    Name = Guid.NewGuid().ToString(),
    //    Components = new List<Component>
    //    {
    //        new Component
    //        {
    //            Id = 1,
    //            Name = "Rtx 6060"
    //        }
    //    }
    //};

    //var request2 = new Request
    //{
    //    Headers = new Dictionary<string, string>
    //    {
    //        ["Route"] = "getNewComponents"
    //    },
    //    Body = JsonSerializer.Serialize(body2)
    //};

    //var response2 = await client.Send(request2);
    //var responseBody2 = response2.GetBody<Result>();

    //Console.WriteLine(response2.GetBody<string>());


}

await Main();
