using System.Reflection;
using System.Text;
using System.Text.Json;
using Lib.Iteraction;
using Lib.Iteraction.Bytes;
using Lib.Iteraction.Enums;
using Lib.Iteraction.Handle;
using Lib.Iteraction.Mutation;
using Lib.Iteraction.Serialization;
using Lib.Iteraction.Validation;
using Schemas;

var btw = new ByteWriter();
var btr = new ByteReader();
var ser = new Serializer(Encoding.Unicode);

var val = new Validator();
var prep = new Preparer();
var sreq = new RequestHandler(val, prep);

var server = new BidtpServer(); 

server.SetPipeName("testPipe");
server.SetProcessPipeQueueDelayTime(1000);

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

