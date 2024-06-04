﻿using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Lib;
using Lib.Iteraction.Validation;
using Lib.Iteraction.Bytes;
using Lib.Iteraction.Serialization;
using Lib.Iteraction.Convert;
using Lib.Iteraction.Mutation;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Handle;
using Lib.Iteraction.Contracts;

namespace Lib.Iteraction
{
    public class BidtpServer : IBidtpServer
    {
        private IValidator _validator;
        private IPreparer _preparer;
        private ISerializer _serializer;
        private IByteWriter _byteWriter;
        private IByteReader _byteReader;
        private IRequestHandler _requestHandler;
        private ILogger _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private string _pipeName;
        private int _processPipeQueueDelayTime;

        public bool IsRunning => _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;

        public BidtpServer()
        {
            _validator = new Validator();
            _preparer = new Preparer();
            _serializer = new Serializer(Encoding.UTF8);
            _byteWriter = new ByteWriter();
            _byteReader = new ByteReader();
            _requestHandler = new RequestHandler(_validator, _preparer);

            _processPipeQueueDelayTime = 100;

            _pipeName = "DefaultPipeName";
        }

        public void AddValidator(IValidator validator)
        {
            _validator = validator;
        }

        public void AddPreparer(IPreparer preparer)
        {
            _preparer = preparer;
        }

        public void AddSerializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void AddByteWriter(IByteWriter byteWriter)
        {
            _byteWriter = byteWriter;
        }

        public void AddByteReader(IByteReader byteReader)
        {
            _byteReader = byteReader;
        }

        public void AddRequestHandler(IRequestHandler requestHandler)
        {
            _requestHandler = requestHandler;
        }

        public void AddLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void SetPipeName(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void SetProcessPipeQueueDelayTime(int processPipeQueueDelayTime)
        {
            _processPipeQueueDelayTime = processPipeQueueDelayTime;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (_cancellationTokenSource != null)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await Task.WhenAll(ProcessPipeQueue(_cancellationTokenSource.Token), ListenForNewConnections(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private NamedPipeServerStream CreatePipeServer()
        {
            var result = new NamedPipeServerStream(_pipeName, PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            return result;
        }

        private async Task ListenForNewConnections(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var pipeServer = CreatePipeServer();

                try
                {
                    await pipeServer.WaitForConnectionAsync(cancellationToken);

                    _ = Task.Run(async () => await HandleClient(pipeServer, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        private async Task HandleClient(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
        {
            try
            {
                var deserializeRequest = await _byteReader.Read(pipeServer);

                var request = await _serializer.DeserializeRequest(deserializeRequest);

                var response = await _requestHandler.ServeRequest(request);

                var serializeRequest = await _serializer.SerializeResponse(response);

                await _byteWriter.Write(serializeRequest, pipeServer);

                _logger?.LogInformation("Response sent");
                _logger?.LogInformation(response.GetBody<string>());
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error: {ex.Message}");
            }
            finally
            {
                pipeServer.Disconnect();
                pipeServer.Dispose();
            }
        }

        private async Task ProcessPipeQueue(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_processPipeQueueDelayTime, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}