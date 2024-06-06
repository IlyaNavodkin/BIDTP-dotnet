using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Build.Contracts
{
    public interface IBidtpServerBuilder
    {
        BidtpServerBuilder AddRoute(string route, params Func<Context, Task>[] handlers);
        BidtpServer Build(string[] args = null);
        BidtpServerBuilder WithByteReader(IByteReader byteReader);
        BidtpServerBuilder WithByteWriter(IByteWriter byteWriter);
        BidtpServerBuilder WithPipeName(string pipeName);
        BidtpServerBuilder WithPreparer(IPreparer preparer);
        BidtpServerBuilder WithProcessPipeQueueDelayTime(int delayTime);
        BidtpServerBuilder WithRequestHandler(IRequestHandler requestHandler);
        BidtpServerBuilder WithSerializer(ISerializer serializer);
        BidtpServerBuilder WithServiceContainer(IServiceProvider services);
        BidtpServerBuilder WithValidator(IValidator validator);
    }
}