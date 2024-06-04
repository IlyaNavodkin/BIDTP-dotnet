using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading.Tasks;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Lib;
using Lib.Iteraction;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.Response;
using Lib.Iteraction.Validation;
using Lib.Iteraction.Preparers;
using Lib.Iteraction.Bytes;
using Lib.Iteraction.Serialization;
using Lib.Iteraction.Convert;
using Lib.Iteraction.Mutation;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Lib.Iteraction.Bytes.Contracts;


namespace Lib.Iteraction
{
    public class ServerBase
    {
        private readonly IValidator _validator;
        private readonly IPreparer _preparer;
        private readonly ISerializer _serializer;
        private readonly IByteWriter _byteWriter;
        private readonly IByteReader _byteReader;
        private readonly IRequestHandler _requestHandler;
        private readonly ILogger _logger;

        private string PipeName { get; }

        private bool _isRunning;
        private int _processPipeQueueDelayTime = 100;

        public ServerBase()
        {
            _validator = new Validator();
            _preparer = new Preparer();
            _serializer = new Serializer(Encoding.UTF8);
            _byteWriter = new ByteWriter();
            _byteReader = new ByteReader();
            _requestHandler = new RequestHa;
            
            _processPipeQueueDelayTime = processPipeQueueDelayTime;

            PipeName = pipeName ?? "testpipe";
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
                RequestRecieved?.Invoke(this, EventArgs.Empty);

                var deserializeRequest = await _byteReader.Read(pipeServer);

                var request = await _serializer.DeserializeRequest(deserializeRequest);

                var response = await _requestHandler.ServeRequest(request);

                var serializeRequest = await _serializer.SerializeResponse(response);

                await _byteWriter.Write(serializeRequest, pipeServer);

                RequestRecieved?.Invoke(this, EventArgs.Empty);

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

                RequestRecieved?.Invoke(this, EventArgs.Empty);
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

        /// <summary>
        ///  Occurs when write progress
        /// </summary>
        public event EventHandler<EventArgs> WriteCompleted;
        /// <summary>
        ///  Occurs when read progress
        /// </summary>
        public event EventHandler<EventArgs> ReadCompleted;

        /// <summary>
        ///  Occurs when message recived
        /// </summary>
        public event EventHandler<EventArgs> RequestRecieved;
        /// <summary>
        ///  Occurs when response pushed
        /// </summary>
        public event EventHandler<EventArgs> ResponsePushed;
        /// <summary>
        ///  Occurs when response pushed
        /// </summary>
        public event EventHandler<EventArgs> RequestIsCompleted;

    }
}
