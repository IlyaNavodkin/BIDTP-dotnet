using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Module.MockableServer;
using Example.Schemas.Dtos;

namespace BIDTP.Dotnet.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net60, invocationCount: 200)]
    [SimpleJob(RuntimeMoniker.Net48, invocationCount: 200)]
    public class BodyJsonConvertBenchmark
    {
        private BidtpServer _server;
        private BidtpClient _client;
        private CancellationTokenSource _clientCancellationTokenSource;
        private CancellationTokenSource _serverCancellationTokenSource;

        private const string PipeName = "testPipe";

        public BodyJsonConvertBenchmark()
        {
            SetUp().Wait();
        }

        private async Task SetUp()
        {
            _server = ServerTestFactory.CreateServer();
            _clientCancellationTokenSource = new CancellationTokenSource();
            _serverCancellationTokenSource = new CancellationTokenSource();

            _server.Start(_serverCancellationTokenSource.Token);

            _client = new BidtpClient();

            _client.Pipename = PipeName;
        }

        [Benchmark]
        public async Task GetBodyFromResponse()
        {
            var request = new Request();
            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");

            request.SetRoute("GetMappedObjectFromObjectContainer");
            request.Headers.Add("Authorization", "adminToken");

            var response = await _client.Send(request);
            
            var body = response.GetBody<AdditionalData>();
        }
    }
}