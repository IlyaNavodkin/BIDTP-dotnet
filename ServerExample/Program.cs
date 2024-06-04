using System.Reflection;
using System.Text;
using System.Text.Json;
using Lib;
using Lib.Iteraction;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Response;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;
using Schemas;

var btw = new ByteWriter();
var btr = new ByteReader();
var ser = new Serializer(Encoding.Unicode);
var val = new Validator();
var prep = new Preparer();
var sreq = new RequestHandler(val, prep);

sreq.AddRoute("getNewComponents", JustChickenGuard);

var server = new ServerBase( val, prep, ser, btw, btr, sreq); 

await server.Start();

Console.ReadKey();

Task JustChickenGuard(Context context)
{
    var request = context.Request;

    var result = new Result
    {
        Data = new List<Component>
        {
            new Component { Id = 1, Name = "Rtx 4070" },
            new Component { Id = 2, Name = "Rtx 4080" },
        },
    };

    var response = new Response(StatusCode.Success)
    {
        Body = JsonSerializer.Serialize(result)
    };

    context.Response = response;

    return Task.CompletedTask;
}

