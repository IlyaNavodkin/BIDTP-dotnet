using System;
using System.Collections.Generic;
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
using System.Linq;
using System.Reflection;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;

namespace BIDTP.Dotnet.Core.Build
{
    public class BidtpServerBuilder : IBidtpServerBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();

        private readonly HashSet<Type> _controllerTypes = new HashSet<Type>();

        private string _pipeName = "DefaultPipeName";
        private int _processPipeQueueDelayTime = 100;

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

        public BidtpServerBuilder WithController<TController>() where TController : class
        {
            if (_controllerTypes.Contains(typeof(TController)))
            {
                throw new InvalidOperationException
                    ($"Controller type {typeof(TController).Name} is already registered.");
            }

            _controllerTypes.Add(typeof(TController));

            return this;
        }

        private BidtpServerBuilder RegisterDefaultServices()
        {
            if (!Services.Any(s => s.ServiceType == typeof(ILogger)))
            {
                Services.AddSingleton<ILogger, ConsoleLogger>();
            }

            if (!Services.Any(s => s.ServiceType == typeof(IValidator)))
            {
                Services.AddSingleton<IValidator, Validator>();
            }

            if (!Services.Any(s => s.ServiceType == typeof(IPreparer)))
            {
                Services.AddSingleton<IPreparer, Preparer>();
            }

            if (!Services.Any(s => s.ServiceType == typeof(ISerializer)))
            {
                Services.AddSingleton<ISerializer, Serializer>();
            }

            if (!Services.Any(s => s.ServiceType == typeof(IByteWriter)))
            {
                Services.AddSingleton<IByteWriter, ByteWriter>();
            }

            if (!Services.Any(s => s.ServiceType == typeof(IByteReader)))
            {
                Services.AddSingleton<IByteReader, ByteReader>();
            }

            if (!Services.Any(s => s.ServiceType == typeof(IRequestHandler)))
            {
                Services.AddSingleton<IRequestHandler, RequestHandler>();
            }

            return this;
        }

        public BidtpServer Build(string[] args = null)
        {
            RegisterDefaultServices();
            RegisterControllers();

            var serviceProvider = Services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger>();

            logger.LogInformation("Building BidtpServer...");

            var requestHandler = serviceProvider.GetService<IRequestHandler>();

            requestHandler.Initialize(serviceProvider, _controllerTypes.ToArray());

            logger.LogInformation("IRequestHandler initialized...");

            var validator = serviceProvider.GetService<IValidator>();
            var preparer = serviceProvider.GetService<IPreparer>();
            var serializer = serviceProvider.GetService<ISerializer>();
            var byteWriter = serviceProvider.GetService<IByteWriter>();
            var byteReader = serviceProvider.GetService<IByteReader>();

            var server = new BidtpServer
            (
                validator,
                preparer,
                serializer,
                byteWriter,
                byteReader,
                requestHandler,
                serviceProvider,
                _pipeName,
                _processPipeQueueDelayTime
            );

            return server;
        }

        private void RegisterControllers()
        {
            foreach (var controllerType in _controllerTypes)
            {
                Services.AddScoped(controllerType);
            }
        }
    }
}
