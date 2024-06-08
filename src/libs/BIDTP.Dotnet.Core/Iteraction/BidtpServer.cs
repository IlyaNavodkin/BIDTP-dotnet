﻿using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using System.Collections.Generic;
using System;
using System.Threading;

namespace BIDTP.Dotnet.Core.Iteraction
{
    public class BidtpServer : IBidtpServer
    {
        public IValidator Validator { get; }
        public IPreparer Preparer { get; }
        public ISerializer Serializer { get; }
        public IByteWriter ByteWriter { get; }
        public IByteReader ByteReader { get; }
        public IRequestHandler RequestHandler { get; }
        public ILogger Logger { get; }

        public IServiceProvider Services { get; }
        public Dictionary<string, Func<Context, Task>[]> RouteHandlers { get; }

        private CancellationTokenSource _cancellationTokenSource;

        public string PipeName;
        public int ProcessPipeQueueDelayTime;

        public BidtpServer(IValidator validator, IPreparer preparer, 
            ISerializer serializer, IByteWriter byteWriter,
            IByteReader byteReader, IRequestHandler requestHandler, 
            ServiceProvider serviceProvider, Dictionary<string, 
            Func<Context, Task>[]> routeHandlers, string pipeName, 
            int processPipeQueueDelayTime)
        {
            Validator = validator;
            Preparer = preparer;
            Serializer = serializer;
            ByteWriter = byteWriter;
            ByteReader = byteReader;
            RequestHandler = requestHandler;

            PipeName = pipeName;
            ProcessPipeQueueDelayTime = processPipeQueueDelayTime;

            RouteHandlers = routeHandlers;

            Services = serviceProvider;
        }

        public bool IsRunning => _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (_cancellationTokenSource != null)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            var logger = Services.GetService<ILogger>();

            logger.LogInformation($"Starting server on pipe: {PipeName}");

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await Task.WhenAll(
                ProcessPipeQueue(_cancellationTokenSource.Token),
                ListenForNewConnections(_cancellationTokenSource.Token)
                );
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private NamedPipeServerStream CreatePipeServer()
        {
            var result = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
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
                var deserializeRequest = await ByteReader.Read(pipeServer);

                var request = await Serializer.DeserializeRequest(deserializeRequest);

                var response = await RequestHandler.ServeRequest(request);

                var serializeRequest = await Serializer.SerializeResponse(response);

                await ByteWriter.Write(serializeRequest, pipeServer);

                Logger?.LogInformation("Response sent");
                Logger?.LogInformation(response.GetBody<string>());
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Error: {ex.Message}");
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
                    await Task.Delay(ProcessPipeQueueDelayTime, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}