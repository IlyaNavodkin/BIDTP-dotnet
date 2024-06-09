using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
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
        try
        {
            if (_routes.Count == 0) throw new Exception("No routes added to the server!");

            var route = request.Headers["Route"];
            _logger.LogInformation($"Request received: {DateTime.Now} Route: {route}");

            if (!_routes.TryGetValue(route, out var routeInfo))
            {
                throw new Exception($"Route '{route}' not found!");
            }

            var actionName = routeInfo.ActionName;
            var controllerType = routeInfo.ControllerType;

            var response = await HandleRequestWithController
                (request, actionName, controllerType);

            var validatedResponse = _validator.ValidateResponse(response);
            var preparedResponse = _preparer.PrepareResponse(validatedResponse);

            return preparedResponse;
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

        var error = new BIDTPError
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

    private async Task<ResponseBase> HandleRequestWithController
        (RequestBase request, string actionName, Type controllerType)
    {
        using var scope = _services.CreateScope();
        var controller = (IController)scope.ServiceProvider.GetRequiredService(controllerType);

        var context = new Context(request, _services);

        await controller.HandleRequest(actionName, context);

        if (context.Response is null) throw new InvalidOperationException("Server response is not set!");

        return context.Response;
    }

    public void Initialize(object[] objects)
    {
        _services = (IServiceProvider)objects[0];

        RegisterAllControllers();
    }

    private void RegisterAllControllers()
    {
        var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttributes<ControllerRouteAttribute>().Any());

        foreach (var controllerType in controllerTypes)
        {
            var controllerRouteAttribute = controllerType.GetCustomAttribute<ControllerRouteAttribute>();
            var controllerRoute = controllerRouteAttribute.Route;

            var methods = controllerType.GetMethods()
                .Where(m => m.GetCustomAttributes<RouteAttribute>().Any());

            foreach (var method in methods)
            {
                var methodRouteAttribute = method.GetCustomAttribute<RouteAttribute>();
                var fullRoute = $"{controllerRoute}/{methodRouteAttribute.Route}";
                _routes[fullRoute] = (controllerType, method.Name);
            }
        }
    }
}