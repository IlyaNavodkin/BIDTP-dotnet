using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Interfaces;
using BIDTP.Dotnet.Core.Iteraction.Providers;

namespace Example.Server.Domain.Auth.Guards;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class RoleGuardAttribute : Attribute, IMethodScopedPreInvokable
{
    private string[] RequiredRoles { get; }

    public RoleGuardAttribute(string requiredRolesListString)
    {
        RequiredRoles = requiredRolesListString.Split(',');
    }

    public Task Invoke(Context context)
    {
        var request = context.Request;
        if (!request.Headers.TryGetValue("Role", out var roleString) || string.IsNullOrEmpty(roleString))
        {
            context.Response = new Response(StatusCode.Unauthorized);
            
            context.Response.SetBody("{ \"Message\": \"Role is not specified\" }");

            return Task.CompletedTask;
        }
         
        if (!RequiredRoles.Contains(roleString))
        {
            context.Response = new Response(StatusCode.Unauthorized);
            
            context.Response.SetBody("{ \"Message\": \"You dont have access\" }");
            
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
