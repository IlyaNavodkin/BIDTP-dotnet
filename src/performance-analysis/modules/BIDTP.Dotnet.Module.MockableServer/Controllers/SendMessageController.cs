using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using BIDTP.Dotnet.Module.MockableServer.Dtos;
using BIDTP.Dotnet.Module.MockableServer.Guards;
using BIDTP.Dotnet.Module.MockableServer.Middlewares;
using Newtonsoft.Json;

namespace BIDTP.Dotnet.Module.MockableServer.Controllers;

public class SendMessageController
{
    [AuthGuard]
    [RoleGuard("admin")]
    public static Task GetMessageForAdmin(Context context)
    {
        var request = context.Request;
        
        if (request.Body.Equals("internal error")) throw new Exception("Mock error");
        
        context.Response = new Response(StatusCode.Success)
        {
            Body = "{ \"Response\": \"" + "Hello admin" + "\" }"
        };
        
        return Task.CompletedTask;
    }
    
    [AuthGuard]
    [RoleGuard("user")]
    public static Task GetMessageForUser(Context context)
    {
        context.Response = new Response(StatusCode.Success)
        {
            Body = "{ \"Response\": \"" + "Hello user" + "\" }"
        };
        
        return Task.CompletedTask;
    }
    
    [AuthGuard]
    public static Task GetAuthAccessResponse(Context context)
    {
        var response = new Response(StatusCode.Success)
        {
            Body = "{ \"Response\": \"" + "Auth access" + "\" }"
        };
        
        context.SetResponse(response);
        
        return Task.CompletedTask;
    }
    
    public static Task GetFreeAccessResponse(Context context)
    {
        var response = new Response(StatusCode.Success)
        {
            Body = "{ \"Response\": \"" + "Free access" + "\" }"
        };
        
        context.SetResponse(response);
        
        return Task.CompletedTask;
    }
    
    [MappingMiddleware(typeof(SimpleObject))]
    public static Task GetMappedObjectWithMetadataFromObjectContainer(Context context)
    {
        var objectContainer = context.ObjectContainer;
        var simpleObject = objectContainer.GetObject<SimpleObject>();
        var additionalData = objectContainer.GetObject<AdditionalData>();
        
        if (simpleObject is null || additionalData is null) throw new Exception("No object in object container");
        
        var response = new Response(StatusCode.Success)
        {
            Body = context.Request.Body
        };
        
        context.SetResponse(response);
        
        return Task.CompletedTask;
    }
}