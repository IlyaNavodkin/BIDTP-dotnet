using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Colors.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Server.Domain.Colors.Controllers;

public class ColorController
{
    public static async Task GetRandomColor(Context context)
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