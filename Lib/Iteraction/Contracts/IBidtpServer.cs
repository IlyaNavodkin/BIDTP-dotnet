using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;

namespace Lib.Iteraction.Contracts
{
    public interface IBidtpServer
    {
        bool IsRunning { get; }
        void SetByteReader(IByteReader byteReader);
        void SetByteWriter(IByteWriter byteWriter);
        void SetLogger(ILogger logger);
        void SetPreparer(IPreparer preparer);
        void SetRequestHandler(IRequestHandler requestHandler);
        void SetSerializer(ISerializer serializer);
        void SetValidator(IValidator validator);
        void SetPipeName(string pipeName);
        void SetProcessPipeQueueDelayTime(int processPipeQueueDelayTime);
        Task Start(CancellationToken cancellationToken = default);
        void Stop();
    }
}