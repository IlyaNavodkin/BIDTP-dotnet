using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Example.Server.Domain.Auth.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Server.Revit.OwnerProcess.Controllers;

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
        
        var getElementsByCategoryRequest = context.Request.GetBody<GetElementsByCategoryRequest>();
        
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

            var response = new Response(StatusCode.Success);
            response.SetBody( dtos );
            
            context.Response = response;
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
        
        var getElementsByCategoryRequest = context.Request.GetBody<DeleteElementRequest>();
        
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

        var message = $"Element with id {getElementsByCategoryRequest.Element.Id} was deleted";
        
        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(message);
    }
}
            