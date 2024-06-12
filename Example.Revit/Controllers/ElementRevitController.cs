using System.Diagnostics;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Schema;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Example.Server.Domain.Auth.Middlewares;
using Example.Server.Domain.Auth.Providers;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External.Handlers;

namespace Example.Revit.Controllers;

[ControllerRoute("ElementRevit")]
public class ElementRevitController : ControllerBase
{
    private readonly AsyncEventHandler _asyncEventHandler;
    private readonly Random _random;

    public ElementRevitController(AsyncEventHandler asyncEventHandler)
    {
        _asyncEventHandler = asyncEventHandler;

        _random = new Random();
    }

    [MethodRoute("GetElements")]
    public async Task GetElements(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var getElementsByCategoryRequest = context.Request.GetBody<GetElementsByCategoryRequest>();

        await _asyncEventHandler.RaiseAsync(application =>
        {
            var selection = application.ActiveUIDocument.Selection;
            var document = application.ActiveUIDocument.Document;

            var pickObjects = selection.PickObjects(ObjectType.Element, "Pick Element");

            var dtos = pickObjects
                .Select(reference => document.GetElement(reference))
                .Where(element =>
                {
                    if (string.IsNullOrEmpty(getElementsByCategoryRequest.Category)) return true;

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
            response.SetBody(dtos);

            context.Response = response;
        });
    }

    [MethodRoute("DeleteElement")]
    public async Task DeleteElement(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var getElementsByCategoryRequest = context.Request.GetBody<DeleteElementRequest>();

        await _asyncEventHandler.RaiseAsync(_ =>
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
    public async Task CreateRandomWall(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var wallCoordinates = context.Request.GetBody<CreateRandomWallLineRequest>();

        var message = string.Empty;
        await _asyncEventHandler.RaiseAsync(_ =>
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

                        message = $" Wall was created successfully. Id: {wall.Id}";
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
    public async Task ChangeWallLocation(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var wallCoordinates = context.Request.GetBody<CreateRandomWallLineRequest>();
        var message = string.Empty;

        await _asyncEventHandler.RaiseAsync(_ =>
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

    [MethodRoute("CreateFloorsColumns")]
    public async Task CreateFloorsColumns(Context context)
    {
        var authService = context.ServiceProvider.GetService<AuthProvider>();

        var authorizationIsValid = await authService.IsAuth(context);

        if (!authorizationIsValid) return;

        var message = string.Empty;
        Response response = null;

        await _asyncEventHandler.RaiseAsync(_ =>
        {
            var document = Nice3point.Revit.Toolkit.Context.Document;

            using (var transaction = new Transaction(document, "Create columns and floors"))
            {
                transaction.Start();

                var columnType = new FilteredElementCollector(document)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_Columns)
                    .Cast<FamilySymbol>()
                    .FirstOrDefault();

                var pickedFloorReference = Nice3point.Revit.Toolkit.Context.UiDocument
                .Selection.PickObject(ObjectType.Element, "Pick Floor for columns");

                var pickedElement = document.GetElement(pickedFloorReference);

                if (pickedElement is null)
                {
                    context.Response = new Response(StatusCode.ClientError);

                    context.Response.SetBody("Failed to get picked element.");

                    return;
                }

                if (pickedElement is not Floor)
                {
                    context.Response = new Response(StatusCode.ClientError);

                    context.Response.SetBody("Picked element is not floor.");

                    return;
                }

                var floorElement = (Floor) pickedElement;

                var floorLevel = (Level) document.GetElement(floorElement.LevelId);

                var geometryOptions = new Options {  ComputeReferences = true, View = document.ActiveView };
                var geometryElement = floorElement.get_Geometry(geometryOptions);

                PlanarFace targetPlanarFace = null;

                foreach (var item in geometryElement)
                {
                    if (item is Solid solid)
                    {
                        foreach (var face in solid.Faces)
                        {
                            if (face is not PlanarFace planarFace) continue;
                            if (!planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ)) continue;

                            targetPlanarFace = planarFace;
                        }
                    }
                }

                if (targetPlanarFace == null)
                {                    
                    context.Response = new Response(StatusCode.ClientError);

                    context.Response.SetBody("Failed to get planar face.");

                    return;
                }

                var vertexes = targetPlanarFace.Triangulate().Vertices;

                var createdColumns = new List<FamilyInstance>();

                foreach (var vertex in vertexes)
                {
                    var createdColumn = document.Create.NewFamilyInstance(vertex, columnType,
                        floorLevel, StructuralType.Column);

                    var offsetValue = _random.Next(-2, 8);

                    createdColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)
                    .Set(offsetValue);

                    createdColumns.Add(createdColumn);
                }

                if (createdColumns.Count > 0)
                {
                    message = $"Columns and floors were created successfully.";

                    transaction.Commit();
                }
                else
                {
                    transaction.RollBack();

                    context.Response = new Response(StatusCode.ClientError);

                    context.Response.SetBody("Columns were not created.");
                }
            }
        });

        context.Response = new Response(StatusCode.Success);
        context.Response.SetBody(message);
    }
}
