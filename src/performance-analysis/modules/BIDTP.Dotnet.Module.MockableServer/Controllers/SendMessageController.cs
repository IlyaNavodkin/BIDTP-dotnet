using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using BIDTP.Dotnet.Module.MockableServer.Middlewares;
using Example.Schemas.Dtos;
using Example.Server.Domain.Auth.Guards;
using Example.Server.Domain.Auth.Middlewares;


namespace BIDTP.Dotnet.Module.MockableServer.Controllers;

[ControllerRoute("SendMessage")]
public class SendMessageController : ControllerBase
{
    [AuthMiddleWare]
    [AuthGuard]
    [RoleGuard("admin")]
    [MethodRoute("GetMessageForAdmin")]
    public Task GetMessageForAdmin(Context context)
    {
        var request = context.Request;

        if (request.GetBody<string>().Equals("internal error")) throw new Exception("Mock error");

        context.Response = new Response(StatusCode.Success);

        context.Response.SetBody("{ \"Response\": \"" + "Hello admin" + "\" }");

        return Task.CompletedTask;
    }

    [AuthMiddleWare]
    [AuthGuard]
    [RoleGuard("user")]
    [MethodRoute("GetMessageForUser")]
    public static Task GetMessageForUser(Context context)
    {
        context.Response = new Response(StatusCode.Success);

        context.Response.SetBody("{ \"Response\": \"" + "Hello user" + "\" }");

        return Task.CompletedTask;
    }

    [AuthMiddleWare]
    [AuthGuard]
    [MethodRoute("GetAuthAccessResponse")]
    public static Task GetAuthAccessResponse(Context context)
    {
        var response = new Response(StatusCode.Success);

        response.SetBody("{ \"Response\": \"" + "Auth access" + "\" }");

        context.Response = response;

        return Task.CompletedTask;
    }

    [AuthMiddleWare]
    [MethodRoute("GetFreeAccessResponse")]
    public static Task GetFreeAccessResponse(Context context)
    {
        var response = new Response(StatusCode.Success);

        response.SetBody("{ \"Response\": \"" + "Free access" + "\" }");

        context.Response = response;

        return Task.CompletedTask;
    }

    [ObjectContainerMiddleware]
    [MethodRoute("GetDataFromStateContainer")]
    public static Task GetDataFromStateContainer(Context context)
    {
        var objectContainer = context.StateContainer;

        var additionalData = objectContainer.GetObject<AdditionalData>();

        if (additionalData is null) throw new Exception("No object in object container");

        var response = new Response(StatusCode.Success);

        response.SetBody(additionalData);

        context.Response = response;

        return Task.CompletedTask;
    }
}