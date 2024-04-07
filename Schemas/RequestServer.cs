using System.Text.Json;
using Lib;
using Lib.Iteraction.Request;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Response;

namespace Schemas;

public class RequestServer : IRequestServer
{
    public async Task<ResponseBase> ServeRequest(RequestBase request)
    {
        if (request.Headers["Route"] != "getNewComponents") return new Response(StatusCode.ServerError);

        var result = new Result
        {
            Data = new List<Component>
            {
                new Component { Id = 1, Name = "Rtx 4070" },
                new Component { Id = 2, Name = "Rtx 4080" },
            }
        };
        var response = new Response(StatusCode.Success)
        {
            Body = JsonSerializer.Serialize(result)
        };
        
        return response;
        
    }
}