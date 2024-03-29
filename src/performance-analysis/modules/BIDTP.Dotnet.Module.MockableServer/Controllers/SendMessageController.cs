﻿using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using BIDTP.Dotnet.Module.MockableServer.Guards;

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
        context.Response = new Response(StatusCode.Success)
        {
            Body = "{ \"Response\": \"" + "Auth access" + "\" }"
        };
        
        return Task.CompletedTask;
    }
    
    public static Task GetFreeAccessResponse(Context context)
    {
        context.Response = new Response(StatusCode.Success)
        {
            Body = "{ \"Response\": \"" + "Free access" + "\" }"
        };
        
        return Task.CompletedTask;
    }
}