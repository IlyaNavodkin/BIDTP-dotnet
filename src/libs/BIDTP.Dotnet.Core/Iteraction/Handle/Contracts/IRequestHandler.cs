using BIDTP.Dotnet.Core.Iteraction.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;

public interface IRequestHandler
{
    Task<ResponseBase> ServeRequest(RequestBase request);
    void Initialize(params object[] objectForInitialize);
}