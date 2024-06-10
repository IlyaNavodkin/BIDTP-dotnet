using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Exceptions;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Schema;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BIDTP.Dotnet.Core.Iteraction.Handle;

public class RequestHandler : IRequestHandler
{
    private IValidator _validator;
    private IPreparer _preparer;
    private ILogger _logger;

    private IServiceProvider _services;
    private Dictionary<string, (Type ControllerType, string ActionName)> _routes 
        = new Dictionary<string, (Type, string)>();

    public RequestHandler(
        IValidator validator,
        IPreparer preparer,
        ILogger logger)
    {
        _validator = validator;
        _preparer = preparer;
        _logger = logger;
    }

    public async Task<ResponseBase> ServeRequest(RequestBase request)
    {
        Context context = null;

        try
        {
            if (_routes.Count == 0) 
                throw new BIDTPException($"No routes added to the server! Use WithController<T>() method to add routes!", 
                    InternalServerErrorType.RouteNotFoundError);

            var route = request.Headers["Route"];
            _logger.LogInformation($"Request received: {DateTime.Now} Route: {route}");

            if (!_routes.TryGetValue(route, out var routeInfo))
            {
                var notFoundResponse = await HandleRouteNotFoundErrorResponse(request);

                return notFoundResponse;
            }

            var actionName = routeInfo.ActionName;
            var controllerType = routeInfo.ControllerType;

            context = new Context(request, _services);
            var response = await HandleRequestWithController(request, actionName, controllerType, context);

            var validatedResponse = _validator.ValidateResponse(response);
            var preparedResponse = _preparer.PrepareResponse(validatedResponse);

            return preparedResponse;
        }
        catch (Exception exception)
        {
            var response = await HandleServerInternalErrorResponse(exception);

            return response;
        }
    }

    private async Task<ResponseBase> HandleServerInternalErrorResponse(Exception exception)
    {
        _logger?.LogCritical(exception, "Internal server error handler!");

        var error = new BIDTPError
        {
            Message = "Internal server error!",
            Description = exception.Message,
            ErrorCode = (int)InternalServerErrorType.RequestHandlerError,
            StackTrace = exception.StackTrace,

        };

        var errorResponse = new Response(StatusCode.ServerError);
        errorResponse.SetBody(error);

        var validateResponser = _validator.ValidateResponse(errorResponse);
        var prepareResponser = _preparer.PrepareResponse(validateResponser);

        return prepareResponser;
    }

    private async Task<ResponseBase> HandleRouteNotFoundErrorResponse(RequestBase request)
    {
        var route = request.Headers[Constants.RouteHeaderName];
        var id = request.Headers[Constants.ResponseProcessIdHeaderName];
        var pid = request.Headers[Constants.ResponseProcessIdHeaderName];

        var message = $"Route '{route}' not found! Request id: {id}, pid: {pid}";

        _logger?.LogWarning(message);

        var error = new BIDTPError
        {
            Message = "Client error!",
            Description = message,
            ErrorCode = (int)InternalServerErrorType.RouteNotFoundError,
            StackTrace = null,

        };

        var errorResponse = new Response(StatusCode.NotFound);
        errorResponse.SetBody(error);

        var validateResponser = _validator.ValidateResponse(errorResponse);
        var prepareResponser = _preparer.PrepareResponse(validateResponser);

        return prepareResponser;
    }

    private async Task<ResponseBase> HandleRequestWithController
        (RequestBase request, string actionName,
        Type controllerType, Context context)
    {
        using var scope = _services.CreateScope();
        var controller = (IController)scope.ServiceProvider.GetRequiredService(controllerType);

        await controller.HandleRequest(actionName, context);

        if (context.Response is null) throw new InvalidOperationException("Server response is not set!");

        return context.Response;
    }

    public void Initialize(object[] objects)
    {
        _services = (IServiceProvider)objects[0];
        
        var controllerTypes = (Type[])objects[1];

        if (controllerTypes is null)
        {
            throw new ArgumentNullException( $"{nameof(controllerTypes)} cannot be null or empty!");
        }

        RegisterAllControllers(controllerTypes);
    }

    private void RegisterAllControllers(Type[] controllerTypes)
    {
        foreach (var controllerType in controllerTypes)
        {
            var controllerRouteAttribute = controllerType.GetCustomAttribute<ControllerRouteAttribute>();

            if (controllerRouteAttribute is null) continue;

            var controllerRoute = controllerRouteAttribute.Route;

            var methods = controllerType.GetMethods()
                .Where(m => m.GetCustomAttributes<MethodRouteAttribute>().Any());

            foreach (var method in methods)
            {
                var methodRouteAttribute = method.GetCustomAttribute<MethodRouteAttribute>();

                if (controllerRouteAttribute.Route == null) continue;

                var fullRoute = $"{controllerRoute}/{methodRouteAttribute.Route}";
                _routes[fullRoute] = (controllerType, method.Name);
            }
        }
    }
}