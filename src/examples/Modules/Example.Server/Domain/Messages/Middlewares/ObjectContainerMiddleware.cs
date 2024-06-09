using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using Example.Schemas.Dtos;

namespace Example.Server.Domain.Messages.Middlewares;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ObjectContainerMiddleware : Attribute, IMethodScopedPreInvokable
{
    public Task Invoke(Context context)
    {
        var request = context.Request;

        var additionalData = request.GetBody<AdditionalData>();

        context.ObjectContainer.AddObject<AdditionalData>(additionalData);

        return Task.CompletedTask;
    }
}