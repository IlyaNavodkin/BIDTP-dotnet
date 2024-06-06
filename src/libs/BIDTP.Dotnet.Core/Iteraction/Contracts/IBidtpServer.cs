using System.Threading;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts
{
    public interface IBidtpServer
    {
        bool IsRunning { get; }
        Task Start(CancellationToken cancellationToken = default);
        void Stop();
    }
}