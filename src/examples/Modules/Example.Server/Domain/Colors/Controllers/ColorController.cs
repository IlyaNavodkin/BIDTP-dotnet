using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Server.Domain.Colors.Controllers;

[ControllerRoute("color")]
public class ColorController : ControllerBase
{
    [MethodRoute("GetRandomColor")]
    public async Task GetRandomColor(Context context)
    {       
        var authService = context.ServiceProvider.GetService<AuthProvider>();
        var getRandomColorRequest = context.ServiceProvider.GetService<ColorProvider>();
        
        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
    
        var message = await getRandomColorRequest.GetRandomColorString();

        context.Response = new Response(StatusCode.Success);
        
        context.Response.SetBody( message );
    }
}