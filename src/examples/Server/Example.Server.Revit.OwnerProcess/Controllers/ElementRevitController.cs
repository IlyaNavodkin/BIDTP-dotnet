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
using Example.Server.Providers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Example.Server.Revit.OwnerProcess.Controllers;

/// <summary>
///  The message controller
/// </summary>
public static class ElementRevitController
{
    private static Regex _regex;
    private static Regex _regex2;
    private static Regex _regex3;

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

    public static async Task GetParameters(Context context)
    {
        var requestDto = context.Request.GetBody<GetParametersRequest>();
        var familyName = requestDto.FamilyName;
        
        var document = Nice3point.Revit.Toolkit.Context.Document;
        var element = new FilteredElementCollector(document)
            .OfClass(typeof(Family))
            .FirstOrDefault(e => e.Name == familyName);

        if (element == null)
        {
            var response = new Response(StatusCode.ClientError)
            {
                Body = "Family not found",
            };
            
            context.Response = response;

            return;
        }

        var family = (Family)element;
        using (var familyDocument = document.EditFamily(family))
        {
            var familyManager = familyDocument.FamilyManager;
            var defaultType = familyManager.CurrentType;
            var parameters = familyManager.GetParameters();

            _regex = new Regex(@"Section #(\d+)");
            _regex2 = new Regex(@"Section #(\d+) Length");
            _regex3 = new Regex(@"Section #(\d+) Height");
            
            var familyParameters = parameters
                .Where(p => requestDto.ParametersMap
                .Any(map => p.Definition.Name == map.RevitParameterName) || PredicateRegex(p))
                .ToList();
            
        }
    }

    private static bool PredicateRegex(FamilyParameter familyParameter)
    {
        var definitionName = familyParameter.Definition.Name;
        var match = _regex.Match(definitionName);
        
        return match.Success;
    }
}
            