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
        void AddByteReader(IByteReader byteReader);
        void AddByteWriter(IByteWriter byteWriter);
        void AddLogger(ILogger logger);
        void AddPreparer(IPreparer preparer);
        void AddRequestHandler(IRequestHandler requestHandler);
        void AddSerializer(ISerializer serializer);
        void AddValidator(IValidator validator);
        void SetPipeName(string pipeName);
        void SetProcessPipeQueueDelayTime(int processPipeQueueDelayTime);
        Task Start(CancellationToken cancellationToken = default);
        void Stop();
    }
}