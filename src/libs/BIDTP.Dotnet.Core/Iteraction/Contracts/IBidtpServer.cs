using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts
{
    public interface IBidtpServer
    {
        bool IsRunning { get; }
        Task Start(CancellationToken cancellationToken = default);
        void Stop();
    }
}