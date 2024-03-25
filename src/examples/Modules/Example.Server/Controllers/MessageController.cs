using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Example.Schemas.Requests;
using Example.Server.Providers;
using Example.Server.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Example.Server.Controllers;

/// <summary>
///  The message controller
/// </summary>
public static class MessageController
{
    /// <summary>
    ///  Print message
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns></returns>
    public static async Task PrintMessageHandler(Context context)
    {       
        var authService = context.ServiceProvider.GetService<AuthProvider>();
        var getRandomColorRequest = context.ServiceProvider.GetService<ColorProvider>();
        
        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
    
        var message = await getRandomColorRequest.GetRandomColorString();
    
        context.Response = new Response(StatusCode.Success)
        {
            Body = "{ \"Color\": \"" + message + "\" }"
        };
        
    }
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
        
        var requestBody = context.Request.Body;
        var getElementsByCategoryRequest = JsonConvert.DeserializeObject<GetElementsByCategoryRequest>(requestBody);
        
        var result = elementRepository.GetElementsByCategory(getElementsByCategoryRequest.Category);
    
        context.Response = new Response(StatusCode.Success)
        {
            Body = JsonConvert.SerializeObject(result)
        };
    }
    /// <summary>
    ///  Mutate context
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns> The response. </returns>
    public static async Task MutateContext(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();
        var elementRepository = context.ServiceProvider.GetService<ElementRepository>();

        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
        
        var requestBody = context.Request.Body;
        var getElementsByCategoryRequest = JsonConvert.DeserializeObject<GetElementsByCategoryRequest>(requestBody);
        
        var result = elementRepository.GetElementsByCategory(getElementsByCategoryRequest.Category);
    
        context.Response = new Response(StatusCode.Success)
        {
            Body = JsonConvert.SerializeObject(result)
        };
    }
}