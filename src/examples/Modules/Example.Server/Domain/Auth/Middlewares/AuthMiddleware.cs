using BIDTP.Dotnet.Core.Iteraction.Providers;

namespace Example.Server.Domain.Auth.Middlewares;

public class AuthMiddleware
{
    public static Task Handle(Context context)
    {
        var request = context.Request;

        int userId = 0;
        string role = null;
        
        request.Headers.TryGetValue("Authorization", out var token);
        
        if(token is null)
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