using System.Reflection;
using System.Text;
using Lib.Iteraction;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;
using Schemas;

var btw = new ByteWriter();
var btr = new ByteReader();
var ser = new Serializer(Encoding.Unicode);
var val = new Validator();
var prep = new Preparer();
var sreq = new RequestHandler();

var server = new ServerBase( val, prep, ser, btw, btr, sreq); 

await server.Start();

Console.ReadKey();
