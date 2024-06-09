using System;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Handle;

namespace BIDTP.Dotnet.Core.Iteraction.Exceptions.Contracts;

public interface IExceptionHandler
{
    Task HandleException(Exception exception, Context context);
}
