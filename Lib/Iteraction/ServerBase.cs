using System.Diagnostics;
using System.IO.Pipes;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;

namespace Lib.Iteraction;

public delegate void DelegateMessage(string Reply);

public class ServerBase
{
    private readonly IValidator _validator;
    private readonly IPreparer _preparer;
    private readonly ISerializer _serializer;
    private readonly IByteWriter _byteWriter;
    private readonly IByteReader _byteReader;
    private readonly IRequestHandler _requestHandler;
    private NamedPipeServerStream _serverPipeStream;
    private string PipeName { get; }
    private bool _isConnected;

    public ServerBase(IValidator validator, 
        IPreparer preparer, ISerializer serializer, 
        IByteWriter byteWriter, IByteReader byteReader, IRequestHandler requestHandler)
    {
        _validator = validator;
        _preparer = preparer;
        _serializer = serializer;
        _byteWriter = byteWriter;
        _byteReader = byteReader;
        _requestHandler = requestHandler;

        PipeName = "testpipe";
    }

    public async Task Start()
    {
        await Listen();
    }

    private async Task Listen()
    {
        CreatePipeServerStream();

        //await _serverPipeStream.WaitForConnectionAsync();

        _isConnected = true;

        while (_isConnected)
        {
            //var deserializeRequest = await _byteReader.Read(_serverPipeStream);



            //var request = await _serializer.DeserializeRequest(deserializeRequest);
            //var response = await _requestHandler.ServeRequest(request);

            //Console.WriteLine("Response get");

            //var validateResponse = _validator.ValidateResponse(response);
            //var preparedResponse = _preparer.PrepareResponse(validateResponse);
            //var serializeRequest = await _serializer.SerializeResponse(preparedResponse);

            //await _byteWriter.Write(serializeRequest, _serverPipeStream);

            //Console.WriteLine("Response sent");
            //Console.WriteLine(preparedResponse.GetBody<string>());
        }
    }

    private void CreatePipeServerStream()
    {
        _serverPipeStream = new NamedPipeServerStream(PipeName,
            PipeDirection.InOut, 2,
            PipeTransmissionMode.Byte);

        var asyncCallback = new AsyncCallback(WaitConnectionCallBack);
        _serverPipeStream.BeginWaitForConnection(asyncCallback, _serverPipeStream);
    }

    public event DelegateMessage? PipeMessage;

    private void WaitConnectionCallBack(IAsyncResult result) 
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;

        var pipeServer = (NamedPipeServerStream)result.AsyncState;

        pipeServer.EndWaitForConnection(result);

        var deserializeRequest =  _byteReader
            .Read(_serverPipeStream)
            .GetAwaiter()
            .GetResult();



        Console.WriteLine($"Thread id: {threadId}");

        Task.Delay(2000).GetAwaiter().GetResult();


        var byte2 = new byte[1024];
        pipeServer.BeginRead(byte2, 0, byte2.Length, new AsyncCallback(AsyncReceive), pipeServer);


        //var request = _serializer
        //    .DeserializeRequest(deserializeRequest)
        //    .GetAwaiter()
        //    .GetResult();


        ////throw new NotImplementedException();

        //var response = _requestHandler
        //    .ServeRequest(request)
        //    .GetAwaiter()
        //    .GetResult();

        //Console.WriteLine("Response get");
        //Console.WriteLine(request.GetBody<string>());

        ////PipeMessage.Invoke(request.GetBody<string>());

        //var validateResponse = _validator.ValidateResponse(response);
        //var preparedResponse = _preparer.PrepareResponse(validateResponse);

        //var serializeRequest = _serializer
        //    .SerializeResponse(preparedResponse)
        //    .GetAwaiter()
        //    .GetResult();

        //_byteWriter.Write(serializeRequest, _serverPipeStream)
        //    .GetAwaiter()
        //    .GetResult();

        //Console.WriteLine("Response sent");
        //Console.WriteLine(preparedResponse.GetBody<string>());

        //pipeServer.Close();
        //pipeServer = null;

        //CreatePipeServerStream();

        return;
    }


    private void AsyncReceive(IAsyncResult ar)
    {
        try
        {
            var pipeStream = (NamedPipeClientStream)ar.AsyncState;

            var deserializeResponse = _byteReader.Read(pipeStream).GetAwaiter().GetResult();
            var response = _serializer.DeserializeResponse(deserializeResponse).GetAwaiter().GetResult();

            var bytesReaded = pipeStream.EndRead(ar);
        }
        catch (Exception oEX)
        {
            Debug.WriteLine(oEX.Message);
        }
    }

    private void AsyncSend(IAsyncResult iar)
    {
        try
        {
            var pipeStream = (NamedPipeClientStream)iar.AsyncState;

            pipeStream.EndWrite(iar);
        }
        catch (Exception oEX)
        {
            Debug.WriteLine(oEX.Message);
        }
    }
}