using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using BIDTP.Dotnet.Iteraction.Response;
using BIDTP.Dotnet.Iteraction.Response.Enums;
using BIDTP.Dotnet.Server.Iteraction;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Example.Server.Providers;
using Example.Server.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nice3point.Revit.Toolkit.External.Handlers;

namespace Example.Server.Revit.Controllers;

/// <summary>
///  The message controller
/// </summary>
public static class ElementRevitController
{
    /// <summary>
    ///  Get elements by category route handler
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns> The response. </returns>
    public static async Task GetElements(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
        
        var requestBody = context.Request.Body;
        var getElementsByCategoryRequest = JsonConvert.DeserializeObject<GetElementsByCategoryRequest>(requestBody);
        
        await SimpleDimpleExternalApplication
        .AsyncEventHandler.RaiseAsync(application => 
        {
            var selection = application.ActiveUIDocument.Selection;
            var document = application.ActiveUIDocument.Document;
        
            var pickObjects = selection.PickObjects(ObjectType.Element, "Pick Element");

            var dtos = pickObjects
                .Select(reference => document.GetElement(reference))
                .Where(element =>
                {
                    if(string.IsNullOrEmpty(getElementsByCategoryRequest.Category)) return true;
            
                    return element.Category.Name == getElementsByCategoryRequest.Category;
                })
                .Select(element =>
                {
                    var dto = new ElementDto
                    {
                        Id = element.Id.IntegerValue,
                        Name = element.Name,
                        Category = element.Category.Name
                    };
            
                    return dto;
                })
                .ToList();
    
            context.Response = new Response(StatusCode.Success)
            {
                Body = JsonConvert.SerializeObject(dtos)
            };
        });
    }
    
    /// <summary>
    ///  Delete element route handler
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns> The response. </returns>
    public static async Task DeleteElement(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);
        
        if(!authorizationIsValid) return;
        
        var requestBody = context.Request.Body;
        var getElementsByCategoryRequest = JsonConvert.DeserializeObject<DeleteElementRequest>(requestBody);
        
        await SimpleDimpleExternalApplication
            .AsyncEventHandler.RaiseAsync(_  =>
            {
                var document = Nice3point.Revit.Toolkit.Context.Document;
            
                using (var transaction = new Transaction(document, "Delete element"))
                {
                    transaction.Start();

                    document.Delete(new ElementId(getElementsByCategoryRequest.Element.Id));

                    transaction.Commit();

                    Debug.WriteLine("Deleted");
                }
            });
        
        context.Response = new Response(StatusCode.Success)
        {
            Body = $"Element with id {getElementsByCategoryRequest.Element.Id} was deleted"
        };
    }
}