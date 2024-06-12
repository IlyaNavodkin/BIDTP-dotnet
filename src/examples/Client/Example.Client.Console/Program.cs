using System.Diagnostics;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Events;

async Task Main()
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    var tasks = new List<Task>();

    for (int i = 0; i < 1; i++)
    {
        tasks.Add(CreateAndSend());
    }

    await Task.WhenAll(tasks);

    stopwatch.Stop();

    Console.WriteLine($"Total time taken: {stopwatch.ElapsedMilliseconds} ms");

    Console.ReadKey();
}

static async Task CreateAndSend()
{
    var client = new BidtpClient();

    //client.RequestSended += (s, e) =>
    //{
    //    var eventArgs = (RequestSendedProgressEventArgs)e;

    //    Console.WriteLine("Request sended");
    //};

    //client.ResponseReceived += (s, e) =>
    //{
    //    var eventArgs = (ResponseReceivedProgressEventArgs)e;

    //    Console.WriteLine("Response received");
    //};

    //client.IsConnected += (s, e) =>
    //{
    //    var eventArgs = (ClientConnectedEventArgs)e;

    //    Console.WriteLine("Client connected");
    //};


    client.Pipename = "testpipe";

    var request = new Request();

    request.SetRoute("apple/sayHello");
    request.SetBody("test");

    var response = await client.Send(request);
    var responseBody = response.GetBody<string>();

    Console.WriteLine(responseBody);


    //var request2 = new Request();

    //request2.SetRoute("apple/sayFuckU");
    //request2.SetBody("test");

    //var response2 = await client.Send(request2);
    //var responseBody2 = response2.GetBody<string>();


    //Console.WriteLine(responseBody2);


    //var request3 = new Request();

    //request3.SetRoute("apple/throwException");
    //request3.SetBody("test");

    //var response3 = await client.Send(request3);
    //var responseBody3 = response3.GetBody<string>();

    //Console.WriteLine(responseBody3);

    //var request4 = new Request();

    //request4.SetRoute("book/sayHello");
    //request4.SetBody("test");

    //var response4 = await client.Send(request4);
    //var responseBody4 = response4.GetBody<string>();


    //Console.WriteLine(responseBody4);


    //var request5 = new Request();

    //request5.SetRoute("book/sayFuckU");
    //request5.SetBody("test");


    //var response5 = await client.Send(request5);
    //var responseBody5 = response5.GetBody<string>();

    //Console.WriteLine(responseBody5);
}


await Main();
