using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Module.MockableServer;
using Example.Schemas.Dtos;

namespace BIDTP.Dotnet.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net60, invocationCount: 200)]
    [SimpleJob(RuntimeMoniker.Net48, invocationCount: 200)]
    public class BodyJsonConvertBenchmark
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

        public BodyJsonConvertBenchmark()
        {
            SetUp().Wait();
        }

        private async Task SetUp()
        {
            _server = ServerTestFactory.CreateServer();
            _clientCancellationTokenSource = new CancellationTokenSource();
            _serverCancellationTokenSource = new CancellationTokenSource();

            _server.StartAsync(_serverCancellationTokenSource.Token);

            var clientOptions =
                new ClientOptions(PipeName, ChunkSize, LifeCheckTimeRate, ReconnectTimeRate, ConnectTimeout);
            _client = new Client(clientOptions);
            await _client.ConnectToServer(_clientCancellationTokenSource);
        }

        [Benchmark]
        public async Task GetBodyFromResponse()
        {
            var request = new Request();
            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");

            request.SetRoute("GetMappedObjectFromObjectContainer");
            request.Headers.Add("Authorization", "adminToken");

            var response = await _client.WriteRequestAsync(request);
            
            var body = response.GetBody<AdditionalData>();
        }
    }
}