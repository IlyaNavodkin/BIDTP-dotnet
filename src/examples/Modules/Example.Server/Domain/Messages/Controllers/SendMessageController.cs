using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using Example.Schemas.Dtos;
using Example.Server.Domain.Auth.Guards;
using Example.Server.Domain.Auth.Middlewares;

namespace Example.Server.Domain.Messages.Controllers;

[ControllerRoute("SendMessage")]
public class SendMessageController
{
    [AuthGuard]
    [RoleGuard("admin")]
    [MethodRoute("GetMessageForAdmin")]
    public Task GetMessageForAdmin(Context context)
    {
        var request = context.Request;
        
        if (request.GetBody<string>().Equals("internal error")) throw new Exception("Mock error");

        context.Response = new Response(StatusCode.Success);
        
        context.Response.SetBody( "{ \"Response\": \"" + "Hello admin" + "\" }" );
        
        return Task.CompletedTask;
    }
    
    [AuthGuard]
    [RoleGuard("user")]
    [MethodRoute("GetMessageForUser")]
    public static Task GetMessageForUser(Context context)
    {
        context.Response = new Response(StatusCode.Success);
        
        context.Response.SetBody( "{ \"Response\": \"" + "Hello user" + "\" }" );
        
        return Task.CompletedTask;
    }
    
    [AuthGuard]
    [MethodRoute("GetAuthAccessResponse")]
    public static Task GetAuthAccessResponse(Context context)
    {
        var response = new Response(StatusCode.Success);
        
        response.SetBody( "{ \"Response\": \"" + "Auth access" + "\" }" );
        
        context.Response = response;
        
        return Task.CompletedTask;
    }
    
    [MethodRoute("GetFreeAccessResponse")]
    public static Task GetFreeAccessResponse(Context context)
    {
        var response = new Response(StatusCode.Success);
        
        response.SetBody( "{ \"Response\": \"" + "Free access" + "\" }" );
        
        context.Response = response;
        
        return Task.CompletedTask;
    }
    
    [MethodRoute("GetMappedObjectWithMetadataFromObjectContainer")]
    public static Task GetMappedObjectWithMetadataFromObjectContainer(Context context)
    {
        var objectContainer = context.ObjectContainer;

        var additionalData = objectContainer.GetObject<AdditionalData>();
        
        if (additionalData is null) throw new Exception("No object in object container");

        var response = new Response(StatusCode.Success);
        
        response.SetBody(additionalData);
        
        context.Response = response;
        
        return Task.CompletedTask;
    }
}