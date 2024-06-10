using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Module.MockableServer;
using Example.Schemas.Dtos;
using NUnit.Framework;

namespace BIDTP.Dotnet.Tests
{
    [TestFixture]
    public class ClienServertIteraction
    {
        private string PipeName;

        private BidtpServer _server;
        private BidtpClient _client;

        private CancellationTokenSource _clientCancellationTokenSource;
        private CancellationTokenSource _serverCancellationTokenSource;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _server = ServerTestFactory.CreateServer();

            PipeName = _server.PipeName;

            _clientCancellationTokenSource = new CancellationTokenSource();
            _serverCancellationTokenSource = new CancellationTokenSource();

            _server.Start(_serverCancellationTokenSource.Token);

            Thread.Sleep(2000);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _serverCancellationTokenSource.Cancel();
            _serverCancellationTokenSource.Dispose();
            _server.Dispose();

            _clientCancellationTokenSource.Cancel();
            _clientCancellationTokenSource.Dispose();
        }

        [Test]
        public async Task WriteRequestAsync_GetMessageForAdmin_SuccessfullyWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();
            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            request.SetRoute("SendMessage/GetMessageForAdmin");
            request.Headers.Add("Authorization", "adminToken");

            var response = await _client.Send(request);

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
        }

        [Test]
        public async Task WriteRequestAsync_GetMessageForAdmin_UnauthorizedWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();
            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");

            request.SetRoute("SendMessage/GetMessageForAdmin");

            var response = await _client.Send(request);

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Unauthorized));
        }

        [Test]
        public async Task WriteRequestAsync_RouteNotExist()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();
            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");

            request.SetRoute("LaLaLa");
            request.Headers.Add("Authorization", "userToken");

            var response = await _client.Send(request);

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.NotFound));
        }

        [Test]
        public async Task WriteRequestAsync_Spam_RouteNotExist()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var tasks = new List<Task<ResponseBase>>();

            for (int i = 0; i < 5; i++)
            {
                var request = new Request();

                request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
                request.SetRoute("LaLaLa");
                request.Headers.Add("Authorization", "userToken");

                var task = _client.Send(request);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                var response = await task;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCode.NotFound));
            }
        }
        [Test]
        public async Task WriteRequestAsync_Spam_AllMessagesRoute()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var tasks = new List<Task<ResponseBase>>();

            var messageForAdminRequest = new Request();

            messageForAdminRequest.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            messageForAdminRequest.SetRoute("SendMessage/GetMessageForAdmin");
            messageForAdminRequest.Headers.Add("Authorization", "adminToken");

            var messageForUserRequest = new Request();

            messageForUserRequest.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            messageForUserRequest.SetRoute("SendMessage/GetMessageForUser");
            messageForUserRequest.Headers.Add("Authorization", "userToken");

            var messageForFreeAccessRequest = new Request();

            messageForFreeAccessRequest.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            messageForFreeAccessRequest.SetRoute("SendMessage/GetFreeAccessResponse");

            tasks.Add(_client.Send(messageForAdminRequest));
            tasks.Add(_client.Send(messageForUserRequest));
            tasks.Add(_client.Send(messageForFreeAccessRequest));

            foreach (var task in tasks)
            {
                var response = await task;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            }
        }

        [Test]
        public async Task WriteRequestAsync_GetMessageForAdmin_InternalErrorWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();

            request.SetBody<string>("internal error");
            request.SetRoute("SendMessage/GetMessageForAdmin");
            request.Headers.Add("Authorization", "adminToken");

            var response = await _client.Send(request);

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.ServerError));
        }

        [Test]
        public async Task WriteRequestAsync_GetMessageForUser_SuccessfullyWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();

            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            request.SetRoute("SendMessage/GetMessageForUser");
            request.Headers.Add("Authorization", "userToken");

            var response = await _client.Send(request);

            var successMessageString = "{ \"Response\": \"" + "Hello user" + "\" }";

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            Assert.That(successMessageString == response.GetBody<string>());
        }

        [Test]
        public async Task WriteRequestAsync_GetAuthAccessResponse_SuccessfullyWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();

            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            request.SetRoute("SendMessage/GetAuthAccessResponse");
            request.Headers.Add("Authorization", "randomToken");

            var response = await _client.Send(request);

            var successMessageString = "{ \"Response\": \"" + "Auth access" + "\" }";

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            Assert.That(successMessageString == response.GetBody<string>());
        }

        [Test]
        public async Task WriteRequestAsync_GetFreeAccessResponse_SuccessfullyWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var request = new Request();

            request.SetBody<string>("{ \"Message\": \"" + "Hello World" + "\" }");
            request.SetRoute("SendMessage/GetFreeAccessResponse");

            var response = await _client.Send(request);

            var successMessageString = "{ \"Response\": \"" + "Free access" + "\" }";

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            Assert.That(successMessageString == response.GetBody<string>());
        }

        [Test]
        public async Task WriteRequestAsync_GetMappedObjectWithMetadataFromObjectContainer_MappingMiddlewareMiddleware_SuccessfullyWritesRequest()
        {
            _client = new BidtpClient();

            _client.Pipename = PipeName;

            var simpleObject = new AdditionalData
            {
                Guid = Guid.NewGuid().ToString(),
                Items = new List<string> { "Item1", "Item2" },
                Name = "Test"
            };

            var request = new Request();

            request.SetRoute("SendMessage/GetMappedObjectWithMetadataFromObjectContainer");
            request.SetBody<AdditionalData>(simpleObject);

            var response = await _client.Send(request);

            var dto = response.GetBody<AdditionalData>();

            Assert.That(dto.Name, Is.EqualTo(simpleObject.Name));
            Assert.That(dto.Items, Is.EqualTo(simpleObject.Items));
            Assert.That(dto.Guid, Is.EqualTo(simpleObject.Guid));

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
        }
    }
}