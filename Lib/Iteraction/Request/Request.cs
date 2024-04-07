using System.Text.Json;

namespace Lib.Iteraction.Request;

public class Request : RequestBase
{
    public override T GetBody<T>()
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)Body;
        }

        return JsonSerializer.Deserialize<T>(Body);
    }

    public override void SetBody<T>(T body)
    {
        if (typeof(T) == typeof(string))
        {
            Body = body as string;
        }
        else
        {
            Body = JsonSerializer.Serialize(body);
        }
    }
}