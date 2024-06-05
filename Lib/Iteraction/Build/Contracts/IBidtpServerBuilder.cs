using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Handle;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;

namespace Lib.Iteraction.Build.Contracts
{
    public interface IBidtpServerBuilder
    {
        BidtpServerBuilder AddRoute(string route, params Func<Context, Task>[] handlers);
        BidtpServer Build(string[]? args = null);
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