using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using Example.Modules.Schemas.Dtos;

namespace BIDTP.Dotnet.Module.MockableServer.Middlewares;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ObjectContainerMiddleware : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {
        var request = context.Request;

        var additionalData = request.GetBody<AdditionalData>();

        context.StateContainer.AddObject(additionalData);

        return Task.CompletedTask;
    }
}