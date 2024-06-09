using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts
{
    public interface IBidtpClient
    {
        Task<ResponseBase> Send(RequestBase request, CancellationToken cancellationToken = default);

        public event EventHandler<EventArgs> RequestSended;
        public event EventHandler<EventArgs> ResponseReceived;
        public event EventHandler<EventArgs> IsConnected;
    }
}