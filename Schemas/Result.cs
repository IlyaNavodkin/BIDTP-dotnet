using System.Data;
using Chamion.Newtonsoft.Json.DataTable.Converters;
using Newtonsoft.Json;

namespace Schemas;

public class Result
{
    public List<Component> Data { get; set; }
    
    [JsonConverter( typeof(DataTableConverter<ComputersRow>))]
    public DataTable DataTableDiscontComputers { get; set; }
}