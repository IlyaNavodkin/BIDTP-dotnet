using System.IO.Pipes;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;
using Lib.Iteraction.Serializator;
using Lib.Iteraction.Validator;

namespace Lib.Iteraction;

public class ClientBase
{
    private readonly IValidator _validator;
    private readonly IPreparer _preparer;
    private readonly ISerializer _serializer;
    private readonly IByteWriter _byteWriter;
    private readonly IByteReader _byteReader;
    private NamedPipeClientStream _clientPipeStream;
    private string PipeName = "testpipe";
    private bool _isConnected;
    
    public ClientBase(IValidator validator, 
        IPreparer preparer, ISerializer serializer, 
        IByteWriter byteWriter, IByteReader byteReader)
    {
        _validator = validator;
        _preparer = preparer;
        _serializer = serializer;
        _byteWriter = byteWriter;
        _byteReader = byteReader;
    }

    public async Task ConnectToServer()
    {
        _clientPipeStream = new NamedPipeClientStream(
            ".", PipeName, PipeDirection.InOut
        );
        
        await _clientPipeStream.ConnectAsync();
    }

    public async Task<ResponseBase> Send(RequestBase request)
    {
        var validRequest = _validator.ValidateRequest(request);
        var preparedRequest = _preparer.PrepareRequest(validRequest);
        var serializeRequest = await _serializer.SerializeRequest(preparedRequest);
        await _byteWriter.Write(serializeRequest, _clientPipeStream);
        
        var deserializeResponse = await _byteReader.Read(_clientPipeStream);
        var response = await _serializer.DeserializeResponse(deserializeResponse);

        return response;
    }
}