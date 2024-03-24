using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Interfaces;
using BIDTP.Dotnet.Core.Iteraction.Providers;

namespace BIDTP.Dotnet.Module.MockableServer.Guards;

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
            context.Response = new Response(StatusCode.Unauthorized)
            {
                Body = "{ \"Message\": \"Role not specified in the request headers\" }"
            };

            return Task.CompletedTask;
        }
         
        if (!RequiredRoles.Contains(roleString))
        {
            context.Response = new Response(StatusCode.Unauthorized)
            {
                Body = "{ \"Message\": \"You don't have permission to access this resource\" }"
            };
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
