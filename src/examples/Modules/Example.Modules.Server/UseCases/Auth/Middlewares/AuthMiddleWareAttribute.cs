using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;


namespace Example.Modules.Server.Domain.Auth.Middlewares;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AuthMiddleWareAttribute : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {
        var request = context.Request;

        int userId = 0;
        string role = null;

        request.Headers.TryGetValue("Authorization", out var token);

        if (token is null)
        {
            return Task.CompletedTask;
        }

        if (token.Equals("userToken"))
        {
            userId = 1;
            role = "user";

            request.Headers.Add("UserId", userId.ToString());
            request.Headers.Add("Role", role);
        }
        else if (token.Equals("adminToken"))
        {
            userId = 2;
            role = "admin";

            request.Headers.Add("UserId", userId.ToString());
            request.Headers.Add("Role", role);
        }
        else if (token.Equals("randomToken"))
        {
            userId = 3;
            role = "normis";

            request.Headers.Add("UserId", userId.ToString());
            request.Headers.Add("Role", role);
        }

        return Task.CompletedTask;
    }
}

