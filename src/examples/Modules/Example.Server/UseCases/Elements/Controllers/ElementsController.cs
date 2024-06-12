using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Example.Server.Domain.Auth.Middlewares;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Elements.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External.Handlers;

namespace Example.Server.Domain.Elements.Controllers;

[ControllerRoute("ElementRevit")]
public class ElementsController : ControllerBase
{
    private readonly AuthProvider _authProvider;
    private readonly ElementRepository _elementRepository;

    public ElementsController(AuthProvider authProvider, 
        ElementRepository elementRepository)
    {
        _authProvider = authProvider;
        _elementRepository = elementRepository;
    }

    [MethodRoute("GetElements")]
    public static async Task GetElements(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();
        var elementRepository = context.ServiceProvider.GetService<ElementRepository>();

        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
        
        var getElementsByCategoryRequest = context.Request.GetBody<GetElementsByCategoryRequest>();
        
        var result = elementRepository.GetElementsByCategory(getElementsByCategoryRequest.Category);

        context.Response = new Response(StatusCode.Success);
        
        context.Response.SetBody( result );
    }

    [MethodRoute("DeleteElement")]
    public async Task DeleteElement(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        var elementDto = context.Request.GetBody<ElementDto>();

        if (!authorizationIsValid) return;

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody($"Element {elementDto.Id} was deleted");
    }

    [SecondCustomMiddleware]
    [FirstCustomMiddleware]
    [MethodRoute("CreateRandomWall")]
    public async Task CreateRandomWall(Context context)
    {
        var wallCoordinates = context.Request.GetBody<CreateRandomWallLineRequest>();

        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var startPoint = wallCoordinates.Line.StartPoint;
        var endPoint = wallCoordinates.Line.EndPoint;

        var message = $"Wall with start point {startPoint.X} | " +
            $"{startPoint.Y} and end point  {endPoint.X} | {endPoint.Y}  was created";

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(message);
    }

    [MethodRoute("CreateFloorsColumns")]
    public async Task CreateFloorsColumns(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var message = $"Created floors and columns";

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(message);
    }
}