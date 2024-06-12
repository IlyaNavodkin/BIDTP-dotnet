using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using Example.Modules.Server.Domain.Auth.Providers;
using Example.Modules.Server.Domain.Colors.Providers;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External.Handlers;
using System.Threading.Tasks;

namespace Example.Modules.Revit.Controllers;

[ControllerRoute("Color")]
public class RevitColorController : ControllerBase
{
    private readonly AsyncEventHandler _asyncEventHandler;

    public RevitColorController(AsyncEventHandler asyncEventHandler)
    {
        _asyncEventHandler = asyncEventHandler;
    }

    [MethodRoute("GetRandomColor")]
    public async Task GetRandomColor(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();
        var getRandomColorProvider = context.ServiceProvider.GetService<ColorProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var result = string.Empty;

        await _asyncEventHandler.RaiseAsync(async _ =>
        {
            var colorName = await getRandomColorProvider.GetRandomColorString();
            result = $"Random color from Revit (Mock this shit): {colorName}";
        });

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(result);
    }
}