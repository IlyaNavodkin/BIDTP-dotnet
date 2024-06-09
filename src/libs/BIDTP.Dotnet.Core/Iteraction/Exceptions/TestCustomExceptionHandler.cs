using System;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Exceptions.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Core.Iteraction.Exceptions;

public class TestCustomExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;

    public TestCustomExceptionHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task HandleException(Exception exception, Context context)
    {
        _logger?.LogCritical("Custom server error handler!");
    }
}
