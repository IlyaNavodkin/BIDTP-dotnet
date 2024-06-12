using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using System.Diagnostics;

namespace Example.Server.Domain.Auth.Middlewares;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class FirstCustomMiddlewareAttribute : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {   
        Debug.WriteLine("Custom First Middleware");

        return Task.CompletedTask;
    }
}
