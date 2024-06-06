using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Logger;
using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using BIDTP.Dotnet.Core.Build.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization;
using BIDTP.Dotnet.Core.Iteraction.Mutation;
using BIDTP.Dotnet.Core.Iteraction.Bytes;
using BIDTP.Dotnet.Core.Iteraction.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Core.Build
{
    public class BidtpServerBuilder : IBidtpServerBuilder
    {
        private IValidator _validator = new Validator();
        private IPreparer _preparer = new Preparer();
        private ISerializer _serializer = new Serializer(Encoding.UTF8);
        private IByteWriter _byteWriter = new ByteWriter();
        private IByteReader _byteReader = new ByteReader();

        private IRequestHandler _requestHandler;

        public IServiceCollection Services { get; } = new ServiceCollection();

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

        public BidtpServer Build(string[] args = null)
        {
            Services.AddSingleton<ILogger, ConsoleLogger>();

            var serviceProvider = Services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger>();

            var server = new BidtpServer
            {
                Validator = _validator,
                Preparer = _preparer,
                Serializer = _serializer,
                ByteWriter = _byteWriter,
                ByteReader = _byteReader,
                RequestHandler = _requestHandler ??
                    new RequestHandler(_validator, _preparer, logger, serviceProvider, _routeHandlers),
                Services = serviceProvider,
                RouteHandlers = _routeHandlers,
                PipeName = _pipeName,
                ProcessPipeQueueDelayTime = _processPipeQueueDelayTime
            };

            return server;
        }
    }
}
