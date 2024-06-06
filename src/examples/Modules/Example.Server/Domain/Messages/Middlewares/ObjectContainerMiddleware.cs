using BIDTP.Dotnet.Core.Iteraction.Handle;
using Example.Schemas.Dtos;

namespace Example.Server.Domain.Messages.Middlewares;

public class ObjectContainerMiddleware
{
    public static Task Handle(Context context)
    {
        var request = context.Request;
        
        var additionalData = request.GetBody<AdditionalData>();
            
        context.ObjectContainer.AddObject<AdditionalData>(additionalData);
        
        return Task.CompletedTask;
    }
}