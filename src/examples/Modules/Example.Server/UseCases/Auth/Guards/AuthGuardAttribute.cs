using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Schema;


namespace Example.Server.Domain.Auth.Guards;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AuthGuardAttribute : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {
        var request = context.Request;

        request.Headers.TryGetValue("UserId", out var userIdString);

        if (string.IsNullOrEmpty(userIdString))
        {
            var dto = new BIDTPError
            {
                Message = "Not authorized",
                Description = "Not authorized ",
                ErrorCode = 403
            };

            var response = new Response(StatusCode.Unauthorized);
            
            response.SetBody(dto);

            context.Response = response;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

