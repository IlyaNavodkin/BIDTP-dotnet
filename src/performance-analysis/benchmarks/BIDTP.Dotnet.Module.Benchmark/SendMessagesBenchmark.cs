using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Module.MockableServer;

namespace BIDTP.Dotnet.Benchmark;

[SimpleJob(RunStrategy.Monitoring, iterationCount: 1000, runtimeMoniker: RuntimeMoniker.Net48)]
[SimpleJob(RunStrategy.Monitoring, iterationCount: 1000, runtimeMoniker: RuntimeMoniker.Net60)]
public class SendMessagesBenchmark
{
    private Server _server;
    private Client _client;
    private CancellationTokenSource _clientCancellationTokenSource;
    private CancellationTokenSource _serverCancellationTokenSource;

    private const string PipeName = "testPipe";
    private const int ChunkSize = 1024;
    private const int LifeCheckTimeRate = 5000;
    private const int ReconnectTimeRate = 10000;
    private const int ConnectTimeout = 5000;

    public SendMessagesBenchmark()
    {
        SetUp().Wait();
    }

    private async Task SetUp()
    {
        _server = ServerTestFactory.CreateServer();
        _clientCancellationTokenSource = new CancellationTokenSource();
        _serverCancellationTokenSource = new CancellationTokenSource();
        
        _server.StartAsync(_serverCancellationTokenSource.Token);
        
        var clientOptions = new ClientOptions(PipeName, ChunkSize, LifeCheckTimeRate, ReconnectTimeRate, ConnectTimeout);
        _client = new Client(clientOptions);
        await _client.ConnectToServer(_clientCancellationTokenSource);
    }
    
    [Benchmark]
    public async Task SingleSendGeneralRequestAndGetResponse()
    {
        var request = new Request
        {
            Body = "{ \"Message\": \"" + "Hello World" + "\" }",
        };
        request.SetRoute("GetMessageForAdmin");
        request.Headers.Add("Authorization", "adminToken");
        
        await _client.WriteRequestAsync(request);
    }
    
    [Benchmark]
    public async Task SpamSendGeneralRequestAndGetResponse()
    {
        for (int i = 0; i < 20; i++)
        {
            var request = new Request
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("GetMessageForAdmin");
            request.Headers.Add("Authorization", "adminToken");
        
            await _client.WriteRequestAsync(request);
        }
    }
}