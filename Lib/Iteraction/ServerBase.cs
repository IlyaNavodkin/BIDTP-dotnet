using System.IO.Pipes;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;

namespace Lib.Iteraction;

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
        _serverPipeStream = new NamedPipeServerStream(PipeName, 
            PipeDirection.InOut, 2, 
            PipeTransmissionMode.Byte);
        
        await _serverPipeStream.WaitForConnectionAsync();
        
        _isConnected = true;
        
        while (_isConnected)
        {
            var deserializeRequest = await _byteReader.Read(_serverPipeStream);
            var request = await _serializer.DeserializeRequest(deserializeRequest);
            var response = await _requestHandler.ServeRequest(request);
            
            var validateResponse = _validator.ValidateResponse(response);
            var preparedResponse = _preparer.PrepareResponse(validateResponse);
            var serializeRequest = await _serializer.SerializeResponse(preparedResponse);
            
            await _byteWriter.Write(serializeRequest, _serverPipeStream);

            _isConnected = false;
        }
    }

}