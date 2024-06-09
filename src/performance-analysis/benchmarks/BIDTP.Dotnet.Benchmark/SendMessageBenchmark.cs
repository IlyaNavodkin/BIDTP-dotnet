using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Module.MockableServer;

namespace BIDTP.Dotnet.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net60, invocationCount: 200)]
    [SimpleJob(RuntimeMoniker.Net48, invocationCount: 200)]
    public class SendMessagesBenchmark
    {
        private BidtpServer _server;
        private BidtpClient _client;
        private CancellationTokenSource _clientCancellationTokenSource;
        private CancellationTokenSource _serverCancellationTokenSource;

        private const string PipeName = "testPipe";

        public SendMessagesBenchmark()
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
        public async Task SingleSendGeneralRequestAndGetResponse()
        {
            for (int i = 0; i < 20; i++)
            {
                var request = new Request();
                request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");

                request.SetRoute("GetMessageForAdmin");
                request.Headers.Add("Authorization", "adminToken");

                await _client.Send(request);
            }
        }
    }
}