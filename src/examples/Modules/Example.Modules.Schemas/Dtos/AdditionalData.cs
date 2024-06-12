using System.Collections.Generic;

namespace Example.Modules.Schemas.Dtos;

public class AdditionalData
{
    public List<string> Items { get; set; }
    public string Guid { get; set; }
    public string Name { get; set; }
}