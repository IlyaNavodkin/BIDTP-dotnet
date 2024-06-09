using System;
using System.Threading;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Events;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts
{
    public interface IBidtpServer
    {
        bool IsRunning { get; }
        Task Start(CancellationToken cancellationToken = default);
        void Stop();

        public event EventHandler<EventArgs> RequestReceived;
        public event EventHandler<EventArgs> ResponseSended;
    }
}