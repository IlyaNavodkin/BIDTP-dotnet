using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using Lib.Iteraction.ByteReader;
using Lib.Iteraction.ByteWriter;
using Lib.Iteraction.Contracts;
using Lib.Iteraction.Preparer;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validator;

namespace Lib.Iteraction;

public class ClientBase
{
    private readonly IValidator _validator;
    private readonly IPreparer _preparer;
    private readonly ISerializer _serializer;
    private readonly IByteWriter _byteWriter;
    private readonly IByteReader _byteReader;

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

    private async Task<NamedPipeClientStream> TryToConnect()
    {
        var clientPipeStream = new NamedPipeClientStream
        (".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        await clientPipeStream.ConnectAsync();

        return clientPipeStream;
    }

    public async Task<ResponseBase> Send(RequestBase request)
    {
        var pipeStream = await TryToConnect();

        var validRequest = _validator.ValidateRequest(request);
        var preparedRequest = _preparer.PrepareRequest(validRequest);
        var serializeRequest = await _serializer.SerializeRequest(preparedRequest);

        await _byteWriter.Write(serializeRequest, pipeStream);
        WriteCompleted?.Invoke(this, EventArgs.Empty);

        var deserializeResponse = await _byteReader.Read(pipeStream);
        ReadCompleted?.Invoke(this, EventArgs.Empty);

        var response = await _serializer.DeserializeResponse(deserializeResponse);

        return response;
    }

    /// <summary>
    ///  Occurs when write progress
    /// </summary>
    public event EventHandler<EventArgs> WriteCompleted;
    /// <summary>
    ///  Occurs when read progress
    /// </summary>
    public event EventHandler<EventArgs> ReadCompleted;
}
