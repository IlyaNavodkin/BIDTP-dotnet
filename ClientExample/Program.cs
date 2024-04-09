// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using Lib.Iteraction;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.Request;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;
using Schemas;


var btw = new ByteWriter();
var btr = new ByteReader();
var ser = new Serializer( Encoding.Unicode);
var val = new Validator();
var prep = new Preparer();

var client = new ClientBase( val, prep, ser, btw, btr);

await client.ConnectToServer();

var body = new Computer
{
    Id = 1,
    Name = "MSI zalupa",
    Components = new List<Component>
    {
        new Component
        {
            Id = 1,
            Name = "Rtx 3060"
        }
    }
};

var request = new Request
{
    Headers = new Dictionary<string, string>
    {
        ["Route"] = "getNewComponents"
    },
    Body =  JsonSerializer.Serialize(body)
};


var response = await client.Send(request);

var responseBody = response.GetBody<Result>();

Console.WriteLine(response.StatusCode.ToString());



