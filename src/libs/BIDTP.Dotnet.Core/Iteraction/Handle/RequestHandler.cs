using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Lib.Iteraction.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Core.Iteraction.Handle;

public class RequestHandler : IRequestHandler
{
    private IValidator _validator;
    private IPreparer _preparer;
    private ILogger _logger;

    private IServiceProvider _services;
    private Dictionary<string, Func<Context, Task>[]> _routes;

    public RequestHandler(
        IValidator validator,
        IPreparer preparer,
        ILogger logger,
        IServiceProvider serviceProvider,
        Dictionary<string, Func<Context, Task>[]> routes)
    {
        _validator = validator;
        _preparer = preparer;
        _logger = logger;
        _services = serviceProvider;
        _routes = routes;
    }

    public async Task<ResponseBase> ServeRequest(RequestBase request)
    {
        try
        {
            if (_routes.Count == 0) throw new Exception("No routes added to the server!");

            var route = request.Headers["Route"];

            _logger.LogInformation($"Request received: {DateTime.Now} Route: {route}");

            var serverRouteNotExist = !_routes.TryGetValue(route, out var handlers);

            if (serverRouteNotExist)
            {
                throw new Exception($"Route '{route}' not found!");
            }

            var response = await HandleGeneralResponse(request, handlers);

            var validateResponser = _validator.ValidateResponse(response);
            var prepareResponser = _preparer.PrepareResponse(validateResponser);

            return prepareResponser;

        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "Internal server error!");

            var errorResponse = await HandleServerInternalErrorResponse(ex);

            return errorResponse;
        }
    }

    private async Task<ResponseBase> HandleServerInternalErrorResponse(Exception exception)
    {
        Console.WriteLine(exception);

        var error = new Error
        {
            Message = "Internal server error!",
            Description = exception.Message,
            ErrorCode = (int)InternalServerErrorType.DispatcherExceptionError,
            StackTrace = exception.StackTrace,

        };

        var errorResponse = new Response(StatusCode.ServerError);
        errorResponse.SetBody(error);

        var validateResponser = _validator.ValidateResponse(errorResponse);
        var prepareResponser = _preparer.PrepareResponse(validateResponser);

        return prepareResponser;
    }

    private async Task<ResponseBase> HandleGeneralResponse(RequestBase request, Func<Context, Task>[] handlers)
    {
        var context = new Context(request, _services);

        foreach (var handler in handlers)
        {
            var methodInfo = handler.Method;

            var attributes = methodInfo
                .GetCustomAttributes(true)
                .Where(atr => atr is IMethodScopedPreInvokable)
                .Cast<IMethodScopedPreInvokable>()
                .ToArray();

            foreach (var attribute in attributes)
            {
                await attribute.Invoke(context);
                if (context.Response != null) break;
            }

            if (context.Response == null)
            {
                await handler(context);
                if (context.Response != null) break;
            }
        }

        if (context.Response is null) throw new InvalidOperationException("Server response is not set!");

        var response = context.Response;

        return response;
    }

}