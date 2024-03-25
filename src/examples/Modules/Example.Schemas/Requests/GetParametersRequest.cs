using System.Collections.Generic;
using Autodesk.Revit.DB;
using Example.Schemas.Dtos;

namespace Example.Schemas.Requests;

public class GetParametersRequest
{
    public string FamilyName { get; set; }
    public List<ParametersMap> ParametersMap { get; set; }
}