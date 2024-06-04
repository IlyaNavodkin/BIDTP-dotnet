using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lib.Iteraction.Services;
using Lib.Iteraction.Services.Contracts;

namespace Lib.Iteraction.Convert;

public class DataTableConverter<T> : JsonConverter<DataTable> where T : new()
{
    private readonly IStructureDataConverter _structureDataConverter
        = new StructureDataConverter();

    public override DataTable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var objects = JsonSerializer.Deserialize<List<T>>(ref reader, options);
        var result = _structureDataConverter.ToDataTable(objects);

        return result;
    }

    public override void Write(Utf8JsonWriter writer, DataTable value, JsonSerializerOptions options)
    {
        var result = _structureDataConverter.ToObjects<T>(value);

        JsonSerializer.Serialize(writer, result, options);
    }
}