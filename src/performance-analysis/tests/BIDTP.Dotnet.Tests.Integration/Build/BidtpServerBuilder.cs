using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lib.Iteraction.Bytes;
using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Contracts;
using Lib.Iteraction.Handle;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation;
using Lib.Iteraction.Validation.Contracts;
using Lib.Iteraction.Logger;
using Lib.Iteraction.Build.Contracts;

namespace Lib.Iteraction.Build
{
    public class BidtpServerBuilder : IBidtpServerBuilder
    {
        private IValidator _validator = new Validator();
        private IPreparer _preparer = new Preparer();
        private ISerializer _serializer = new Serializer(Encoding.UTF8);
        private IByteWriter _byteWriter = new ByteWriter();
        private IByteReader _byteReader = new ByteReader();

        private IRequestHandler _requestHandler;

        private IServiceProvider _services;

        private Dictionary<string, Func<Context, Task>[]> _routeHandlers = new();

        private string _pipeName = "DefaultPipeName";
        private int _processPipeQueueDelayTime = 100;

        public BidtpServerBuilder WithValidator(IValidator validator)
        {
            _validator = validator;
            return this;
        }

        public BidtpServerBuilder WithPreparer(IPreparer preparer)
        {
            _preparer = preparer;
            return this;
        }

        public BidtpServerBuilder WithSerializer(ISerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public BidtpServerBuilder WithByteWriter(IByteWriter byteWriter)
        {
            _byteWriter = byteWriter;
            return this;
        }

        public BidtpServerBuilder WithByteReader(IByteReader byteReader)
        {
            _byteReader = byteReader;
            return this;
        }

        public BidtpServerBuilder WithRequestHandler(IRequestHandler requestHandler)
        {
            _requestHandler = requestHandler;
            return this;
        }

        public BidtpServerBuilder WithServiceContainer(IServiceProvider services)
        {
            _services = services;
            return this;
        }

        public BidtpServerBuilder AddRoute(string route, params Func<Context, Task>[] handlers)
        {
            _routeHandlers.Add(route, handlers);
            return this;
        }

        public BidtpServerBuilder WithPipeName(string pipeName)
        {
            _pipeName = pipeName;
            return this;
        }

        public BidtpServerBuilder WithProcessPipeQueueDelayTime(int delayTime)
        {
            _processPipeQueueDelayTime = delayTime;
            return this;
        }

        public BidtpServer Build(string[]? args = null)
        {
            if (_services == null)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection
                    .AddSingleton<ILogger, ConsoleLogger>();

                _services = serviceCollection.BuildServiceProvider();
            }

            var logger = _services.GetService<ILogger>();

            var server = new BidtpServer
            {
                Validator = _validator,
                Preparer = _preparer,
                Serializer = _serializer,
                ByteWriter = _byteWriter,
                ByteReader = _byteReader,
                RequestHandler = _requestHandler ??
                    new RequestHandler(_validator, _preparer, logger, _services, _routeHandlers),
                Services = _services,
                RouteHandlers = _routeHandlers,
                PipeName = _pipeName,
                ProcessPipeQueueDelayTime = _processPipeQueueDelayTime
            };

            return server;
        }
    }
}
