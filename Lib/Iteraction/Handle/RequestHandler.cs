using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.Json;
using Lib.Iteraction.Contracts;
using Lib.Iteraction.Enums;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lib.Iteraction.Handle;

public class RequestHandler : IRequestHandler
{
    private readonly IValidator _validator;
    private readonly IPreparer _preparer;
    private readonly ILogger<RequestHandler> _logger;

    private readonly Dictionary<string, Func<Context, Task>[]> _routeHandlers = new();
    private readonly ConcurrentDictionary<string, IHostedService> _workers = new();

    public IServiceProvider Services { get; }

    public RequestHandler(IValidator validator, IPreparer preparer)
    {
        _validator = validator;
        _preparer = preparer;
    }

    /// <summary>
    ///  Add a route handler or a group of handlers to the server
    /// </summary>
    /// <param name="route"> The route. </param>
    /// <param name="handlers"> The handler or array of handlers.</param>
    /// <returns> The server builder. </returns>
    public void AddRoute(string route, params Func<Context, Task>[] handlers)
    {
        _routeHandlers.Add(route, handlers);
    }


    /// <summary>
    ///  Add a background service to the server
    /// </summary>
    /// <param name="workerName"> The name of the worker. </param>
    /// <typeparam name="T"> The type of the worker. </typeparam>
    public void AddBackgroundService<T>(string workerName = null) where T : BackgroundService
    {
        var backgroundServiceInstance = ActivatorUtilities.CreateInstance<T>(Services);

        workerName ??= typeof(T).Name;

        if (_workers.TryAdd(workerName, backgroundServiceInstance))
        {
            backgroundServiceInstance.StartAsync(CancellationToken.None);
        }
    }

    /// <summary>
    ///  Stop a background service from the server
    /// </summary>
    /// <param name="workerName"> The name of the worker. </param>
    /// <typeparam name="T"> The type of the worker. </typeparam>
    public void StopBackgroundService<T>(string workerName) where T : BackgroundService
    {
        workerName ??= typeof(T).Name;

        if (_workers.TryRemove(workerName, out var worker))
        {
            worker.StopAsync(CancellationToken.None);
        }
    }

    public async Task<ResponseBase> ServeRequest(RequestBase request)
    {
        try
        {
            if (_routeHandlers.Count == 0) throw new Exception("No routes added to the server!");

            var route = request.Headers["Route"];

            if (route is null) throw new Exception("Route key is not found. Add route header in the request!");

            var serverRouteNotExist = !_routeHandlers.TryGetValue(route, out var handlers);

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
        var context = new Context(request, Services);

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