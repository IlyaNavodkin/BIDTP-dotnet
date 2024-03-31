using System.Collections.Generic;

namespace Example.Schemas.Dtos;

public class AdditionalData
{
    public List<string> Items { get; set; }
    public string Guid { get; set; }
    public string Name { get; set; }
}