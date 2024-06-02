using CALLBACK_CLIENT;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;

class PipeClient
{
    static async Task Main()
    {
        int numClients = 2;
        Thread[] clientThreads = new Thread[numClients];

        var results = new List<string>();

        var start = DateTime.Now;
        for (int i = 0; i < numClients; i++)
        {
            var client = new CallBackPipeClient();

            client.MessageReceived += (sender, message) => Console.WriteLine($"EVENT Client {i} received: {message}");

            var res1 = client.SendMessage($"A{i}").Result;
            var res2 = client.SendMessage($"B{i}").Result;

            results.Add(res1);
            results.Add(res2);

            foreach (var res in results)
            {
                Console.WriteLine(res);
            }

            var res3 = client.SendMessage($"РУДДЩЩЩЩЩЩЩ").Result;
        }
        var end = DateTime.Now;
        var diff = end - start;
        Console.WriteLine(diff);

        Console.WriteLine("All clients have finished interaction.");
        Console.ReadKey();
    }
}
