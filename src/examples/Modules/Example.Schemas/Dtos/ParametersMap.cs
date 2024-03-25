using Autodesk.Revit.DB;

namespace Example.Schemas.Dtos;

public class ParametersMap
{
    public string RevitParameterName { get; set; }
    public ParameterType  ParameterType { get; set; }
    public string ModelPropertyName { get; set; }
}