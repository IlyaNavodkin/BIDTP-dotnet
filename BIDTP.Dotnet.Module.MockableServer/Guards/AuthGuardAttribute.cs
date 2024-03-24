using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Interfaces;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Newtonsoft.Json;

namespace BIDTP.Dotnet.Module.MockableServer.Guards;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AuthGuardAttribute : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {
        var request = context.Request;

        request.Headers.TryGetValue("UserId", out var userIdString);

        if (string.IsNullOrEmpty(userIdString))
        {
            var dto = new Error
            {
                Message = "Not authorized",
                Description = "Not authorized ",
                ErrorCode = 403
            };

            var response = new Response(StatusCode.Unauthorized)
            {
                Body = JsonConvert.SerializeObject(dto)
            };

            context.Response = response;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
