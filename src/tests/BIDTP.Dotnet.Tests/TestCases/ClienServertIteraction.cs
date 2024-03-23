using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Tests.Server;

namespace BIDTP.Dotnet.Tests.TestCases
{
    [TestFixture]
    public partial class ClienServertIteraction
    {
        private const string PipeName = "testPipe";
        private const int ChunkSize = 1024;
        private const int LifeCheckTimeRate = 5000;
        private const int ReconnectTimeRate = 10000;
        private const int ConnectTimeout = 5000;

        private Core.Iteraction.Server _server;
        private Client _client;
        private CancellationTokenSource _cancellationTokenSource;
        
        public ClienServertIteraction()
        {
            _server = ServerTestFactory.CreateServer();
            _cancellationTokenSource = new CancellationTokenSource();
            _server.StartAsync(_cancellationTokenSource.Token);

            var clientOptions = new ClientOptions(PipeName, ChunkSize, LifeCheckTimeRate, ReconnectTimeRate, ConnectTimeout);
            _client = new Client(clientOptions);
            _client.ConnectToServer(_cancellationTokenSource);
        }
        
        [Test]
        public async Task WriteRequestAsync_GetMessageForAdmin_SuccessfullyWritesRequest()
        {
            var request = new Request()
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("GetMessageForAdmin");
            request.Headers.Add("Authorization", "adminToken");

            var response = await _client.WriteRequestAsync(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
        }
        
        [Test]
        public async Task WriteRequestAsync_GetMessageForAdmin_UnauthorizedWritesRequest()
        {
            var request = new Request
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("GetMessageForAdmin");

            var response = await _client.WriteRequestAsync(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Unauthorized));
        }

        [Test]
        public async Task WriteRequestAsync_RouteNotExist()
        {
            var request = new Request
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("LaLaLa");
            request.Headers.Add("Authorization", "userToken");
            
            var response = await _client.WriteRequestAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.NotFound));
        }
        
        [Test]
        public async Task WriteRequestAsync_Spam_RouteNotExist()
        {
            var tasks = new List<Task<Response>>();

            for (int i = 0; i < 5; i++)
            {
                var request = new Request
                {
                    Body = "{ \"Message\": \"" + "Hello World" + "\" }",
                };
                request.SetRoute("LaLaLa");
                request.Headers.Add("Authorization", "userToken");
                
                var task = _client.WriteRequestAsync(request);
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
            var tasks = new List<Task<Response>>();

            var messageForAdminRequest = new Request()
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            messageForAdminRequest.SetRoute("GetMessageForAdmin");
            messageForAdminRequest.Headers.Add("Authorization", "adminToken");

            var messageForUserRequest = new Request()
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            
            messageForUserRequest.SetRoute("GetMessageForUser");
            messageForUserRequest.Headers.Add("Authorization", "userToken");
            
            var messageForFreeAccessRequest = new Request
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            messageForFreeAccessRequest.SetRoute("GetFreeAccessResponse");
            
            tasks.Add(_client.WriteRequestAsync(messageForAdminRequest));
            tasks.Add(_client.WriteRequestAsync(messageForUserRequest));
            tasks.Add(_client.WriteRequestAsync(messageForFreeAccessRequest));
            
            foreach (var task in tasks)
            {
                var response = await task;
                Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            }
        }
        
        [Test]
        public async Task WriteRequestAsync_GetMessageForAdmin_InternalErrorWritesRequest()
        {
            var request = new Request
            {
                Body = "internal error",
            };
            request.SetRoute("GetMessageForAdmin");
            request.Headers.Add("Authorization", "adminToken");

            var response = await _client.WriteRequestAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.ServerError));
        }
        [Test]
        public async Task WriteRequestAsync_GetMessageForUser_SuccessfullyWritesRequest()
        {
            var request = new Request()
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("GetMessageForUser");
            request.Headers.Add("Authorization", "userToken");

            var response = await _client.WriteRequestAsync(request);

            var successMessageString = "{ \"Response\": \"" + "Hello user" + "\" }";

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            Assert.That(successMessageString == response.Body);
        }
        [Test]
        public async Task WriteRequestAsync_GetAuthAccessResponse_SuccessfullyWritesRequest()
        {
            var request = new Request()
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("GetAuthAccessResponse");
            request.Headers.Add("Authorization", "randomToken");

            var response = await _client.WriteRequestAsync(request);

            var successMessageString = "{ \"Response\": \"" + "Auth access" + "\" }";

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            Assert.That(successMessageString == response.Body);
        }
        
        [Test]
        public async Task WriteRequestAsync_GetFreeAccessResponse_SuccessfullyWritesRequest()
        {
            var request = new Request()
            {
                Body = "{ \"Message\": \"" + "Hello World" + "\" }",
            };
            request.SetRoute("GetFreeAccessResponse");
            
            var response = await _client.WriteRequestAsync(request);

            var successMessageString = "{ \"Response\": \"" + "Free access" + "\" }";

            Assert.That(response.StatusCode, Is.EqualTo(StatusCode.Success));
            Assert.That(successMessageString == response.Body);
        }
    }
}
