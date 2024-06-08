using BIDTP.Dotnet.Core.Build;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Build.Contracts
{
    public interface IBidtpServerBuilder
    {
        BidtpServerBuilder AddRoute(string route, params Func<Context, Task>[] handlers);
        BidtpServer Build(string[] args = null);
        BidtpServerBuilder WithPipeName(string pipeName);
        BidtpServerBuilder WithProcessPipeQueueDelayTime(int delayTime);
    }
}