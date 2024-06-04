using System.Text.Json;
using Lib.Iteraction.Contracts;
using Lib.Iteraction.Enums;

namespace Lib.Iteraction;

public class Response : ResponseBase
{
    public Response(StatusCode statusCode) : base(statusCode)
    {
    }

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