using System.Data;
using System.Text.Json.Serialization;
using Lib.Iteraction.Converters;

namespace Schemas;

public class Result
{
    public List<Component> Data { get; set; }
    
    [JsonConverter( typeof(DataTableConverter<ComputersRow>))]
    public DataTable DataTableDiscontComputers { get; set; }
}