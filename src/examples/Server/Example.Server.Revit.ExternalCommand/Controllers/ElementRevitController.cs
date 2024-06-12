using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Example.Server.Domain.Auth.Middlewares;
using Example.Server.Domain.Auth.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Server.Revit.ExternalCommand.Controllers;

[ControllerRoute("ElementRevit")]
public class ElementRevitController : ControllerBase
{
    /// <summary>
    ///  Get elements by category route handler
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns> The response. </returns>
    [MethodRoute("GetElements")]
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
    [MethodRoute("DeleteElement")]
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

    [SecondCustomMiddleware]
    [FirstCustomMiddleware]
    [MethodRoute("CreateRandomWall")]
    public static async Task CreateRandomWall(Context context)
    {
        var wallCoordinates = context.Request.GetBody<CreateRandomWallLineRequest>();

        var message = string.Empty;
        await SimpleDimpleExternalApplication
            .AsyncEventHandler.RaiseAsync(_ =>
            {
                var document = Nice3point.Revit.Toolkit.Context.Document;

                using (var transaction = new Transaction(document, "Create random wall"))
                {
                    transaction.Start();

                    var wallType = new FilteredElementCollector(document)
                         .OfClass(typeof(WallType))
                         .Cast<WallType>()
                         .FirstOrDefault();

                    var level = new FilteredElementCollector(document)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault();

                    var startPoint = new XYZ(wallCoordinates.Line.StartPoint.X, -wallCoordinates.Line.StartPoint.Y, 0);
                    var endPoint = new XYZ(wallCoordinates.Line.EndPoint.X, -wallCoordinates.Line.EndPoint.Y, 0);

                    if (wallType != null && level != null)
                    {
                        var line = Line.CreateBound(startPoint, endPoint);

                        var wall = Wall.Create(document, line, wallType.Id, level.Id, 10.0, 0.0, false, false);

                        wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(wallCoordinates.Line.Guid);

                        transaction.Commit();

                        message = wall.Id.ToString();
                    }
                    else
                    {
                        transaction.RollBack();

                        message = $"Wall was not created";
                    }
                }
            });

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(message);
    }

    [MethodRoute("ChangeWallLocation")]
    public static async Task ChangeWallLocation(Context context)
    {
        var wallCoordinates = context.Request.GetBody<CreateRandomWallLineRequest>();
        var message = string.Empty;

        await SimpleDimpleExternalApplication
            .AsyncEventHandler.RaiseAsync(async _ =>
            {
               

                var document = Nice3point.Revit.Toolkit.Context.Document;

                using (var transaction = new Transaction(document, "Change wall location"))
                {
                    transaction.Start();


                    var existWalls = new FilteredElementCollector(document)
                        .OfClass(typeof(Wall))
                        .Cast<Wall>()
                        .ToList();

                    var existWall = existWalls.FirstOrDefault(e => e.Id.ToString() == wallCoordinates.Line.ElementId);

                    if (existWall != null)
                    {
                        var locationCurve = existWall.Location as LocationCurve;

                        if (locationCurve != null)
                        {
                            var newStartPoint = new XYZ(wallCoordinates.Line.StartPoint.X, -wallCoordinates.Line.StartPoint.Y, 0);
                            var newEndPoint = new XYZ(wallCoordinates.Line.EndPoint.X, -wallCoordinates.Line.EndPoint.Y, 0);

                            var newLine = Line.CreateBound(newStartPoint, newEndPoint);
                            locationCurve.Curve = newLine;

                            message = $"Wall location was changed successfully.";
                        }
                        else
                        {
                            message = "Failed to cast wall location to LocationCurve.";
                        }

                        transaction.Commit();
                    }
                    else
                    {
                        transaction.RollBack();
                        message = $"Wall was not found.";
                    }
                }
            });

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(message);
    }
}
