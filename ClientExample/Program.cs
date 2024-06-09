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
using BIDTP.Dotnet.Core.Iteraction.Events;

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
    var client = new BidtpClient();

    client.RequestSended += (s, e) => 
    {    
        var eventArgs = (RequestSendedProgressEventArgs)e;

        Console.WriteLine("Request sended");
    };

    client.ResponseReceived += (s, e) =>
    {
        var eventArgs = (ResponseReceivedProgressEventArgs)e;

        Console.WriteLine("Response received");
    };

    client.IsConnected += (s, e) =>
    {
        var eventArgs = (ClientConnectedEventArgs)e;

        Console.WriteLine("Client connected");
    };


    client.Pipename = "testpipe";

    var request = new Request();

    request.SetRoute("PrintMessage");
    request.SetBody("test");

    var response = await client.Send(request);
    var responseBody = response.GetBody<string>();

    Console.WriteLine(responseBody);
}


await Main();
