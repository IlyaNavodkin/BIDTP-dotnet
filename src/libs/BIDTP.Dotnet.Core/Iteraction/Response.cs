﻿using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Helpers;

namespace BIDTP.Dotnet.Core.Iteraction;

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

        return JsonSerializer.Deserialize<T>(Body, JsonHelper.GetDefaultJsonSerializerOptions());
    }

    public override void SetBody<T>(T body)
    {
        if (typeof(T) == typeof(string))
        {
            Body = body as string;
        }
        else
        {
            Body = JsonSerializer.Serialize(body, JsonHelper.GetDefaultJsonSerializerOptions());
        }
    }
}