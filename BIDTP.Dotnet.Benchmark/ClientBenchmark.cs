using BenchmarkDotNet.Attributes;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Tests.Server;

namespace BIDTP.Dotnet.Benchmark;

[MemoryDiagnoser]
public class ClientBenchmark
{
    private Server _server;
    private Client _client;
    private CancellationTokenSource _clientCancellationTokenSource;
    private CancellationTokenSource _serverCancellationTokenSource;
    private Request _request;

    private const string PipeName = "testPipe";
    private const int ChunkSize = 1024;
    private const int LifeCheckTimeRate = 5000;
    private const int ReconnectTimeRate = 10000;
    private const int ConnectTimeout = 5000;

    public ClientBenchmark()
    {

    }

    [Benchmark]
    public async Task WriteRequestAsync_GetMessageForAdmin()
    {
        await Task.Delay(5000);
        _server = ServerTestFactory.CreateServer();
        _clientCancellationTokenSource = new CancellationTokenSource();
        _serverCancellationTokenSource = new CancellationTokenSource();
        
        _server.StartAsync(_serverCancellationTokenSource.Token);
        
        await Task.Delay(5000);
        
        var clientOptions = new ClientOptions(PipeName, ChunkSize, LifeCheckTimeRate, ReconnectTimeRate, ConnectTimeout);
        _client = new Client(clientOptions);
        await _client.ConnectToServer(_clientCancellationTokenSource);
        
        _request = new Request
        {
            Body = "{ \"Message\": \"" + "Hello World" + "\" }",
        };
        _request.SetRoute("GetMessageForAdmin");
        _request.Headers.Add("Authorization", "adminToken");

        
        await _client.WriteRequestAsync(_request);
        
        await Task.Delay(5000);
    }
    
    // [Benchmark]
    // public async Task WriteRequestAsync_GetMessageForUser()
    // {
    //     _request = new Request
    //     {
    //         Body = "{ \"Message\": \"" + "Hello World" + "\" }",
    //     };
    //     _request.SetRoute("GetMessageForUser");
    //     _request.Headers.Add("Authorization", "userToken");
    //
    //     await _client.ConnectToServer(_clientCancellationTokenSource);
    //     await _client.WriteRequestAsync(_request);
    // }
}