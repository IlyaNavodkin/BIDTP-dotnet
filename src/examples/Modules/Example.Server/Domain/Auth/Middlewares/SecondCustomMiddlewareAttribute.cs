using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using System.Diagnostics;

namespace Example.Server.Domain.Auth.Middlewares;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class SecondCustomMiddlewareAttribute : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {   
        Debug.WriteLine("Custom Second Middleware");

        return Task.CompletedTask;
    }
}