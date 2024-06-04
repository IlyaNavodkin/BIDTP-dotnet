using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading.Tasks;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;

namespace Lib.Iteraction
{
    public delegate void DelegateMessage(string Reply);

    public class ServerBase
    {
        private readonly IValidator _validator;
        private readonly IPreparer _preparer;
        private readonly ISerializer _serializer;
        private readonly IByteWriter _byteWriter;
        private readonly IByteReader _byteReader;
        private readonly IRequestHandler _requestHandler;

        private string PipeName { get; }
        private bool _isRunning;
        private int _processPipeQueueDelayTime = 100;

        public ServerBase(IValidator validator,
            IPreparer preparer, ISerializer serializer,
            IByteWriter byteWriter, IByteReader byteReader, 
            IRequestHandler requestHandler)
        {
            _validator = validator;
            _preparer = preparer;
            _serializer = serializer;
            _byteWriter = byteWriter;
            _byteReader = byteReader;
            _requestHandler = requestHandler;

            PipeName = "testpipe";
        }

        public async Task Start()
        {
            _isRunning = true;

            await Task.WhenAll(ProcessPipeQueue(), ListenForNewConnections());
        }

        private NamedPipeServerStream CreatePipeServer()
        {
            var result = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 
                NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            return result;
        }

        private async Task ListenForNewConnections()
        {
            while (_isRunning)
            {
                var pipeServer = CreatePipeServer();

                await pipeServer.WaitForConnectionAsync();

                _ = Task.Run(async () => await HandleClient(pipeServer));
            }
        }

        private async Task HandleClient(NamedPipeServerStream pipeServer)
        {
            try
            {
                var deserializeRequest = await _byteReader.Read(pipeServer);

                var request = await _serializer.DeserializeRequest(deserializeRequest);

                var response = await _requestHandler.ServeRequest(request);

                var serializeRequest = await _serializer.SerializeResponse(response);

                await _byteWriter.Write(serializeRequest, pipeServer);

                Console.WriteLine("Response sent");
                Console.WriteLine(response.GetBody<string>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                pipeServer.Disconnect();
                pipeServer.Dispose();
            }
        }

        private async Task ProcessPipeQueue()
        {
            while (_isRunning)
            {
                await Task.Delay(_processPipeQueueDelayTime); 
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
