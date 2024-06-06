using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using Example.Schemas.Requests;
using Example.Server.Domain.Auth.Providers;
using Example.Server.Domain.Elements.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Server.Domain.Elements.Controllers;

public class ElementController
{
    /// <summary>
    ///  Get elements by category route handler
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns> The response. </returns>
    public static async Task GetElements(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();
        var elementRepository = context.ServiceProvider.GetService<ElementRepository>();

        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
        
        var getElementsByCategoryRequest = context.Request.GetBody<GetElementsByCategoryRequest>();
        
        var result = elementRepository.GetElementsByCategory(getElementsByCategoryRequest.Category);

        context.Response = new Response(StatusCode.Success);
        
        context.Response.SetBody( result );
    }
}