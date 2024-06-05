using BIDTP.Dotnet.Core.Iteraction.Bytes;
using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction;

public class BidtpClient : IBidtpClient
{
    private IValidator _validator;
    private IPreparer _preparer;
    private ISerializer _serializer;
    private IByteWriter _byteWriter;
    private IByteReader _byteReader;
    private ILogger _logger;

    private string _pipeName;
    private bool _isConnected;

    public BidtpClient()
    {
        _validator = new Validator();
        _preparer = new Preparer();
        _serializer = new Serializer(Encoding.UTF8);
        _byteWriter = new ByteWriter();
        _byteReader = new ByteReader();

        _pipeName = "DefaultPipeName";
    }

    public void AddValidator(IValidator validator)
    {
        _validator = validator;
    }

    public void AddPreparer(IPreparer preparer)
    {
        _preparer = preparer;
    }

    public void AddSerializer(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public void AddByteWriter(IByteWriter byteWriter)
    {
        _byteWriter = byteWriter;
    }

    public void AddByteReader(IByteReader byteReader)
    {
        _byteReader = byteReader;
    }

    public void AddLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void SetPipeName(string pipeName)
    {
        _pipeName = pipeName;
    }

    private async Task<NamedPipeClientStream> TryToConnect(CancellationToken cancellationToken = default)
    {
        var clientPipeStream = new NamedPipeClientStream
        (".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        await clientPipeStream.ConnectAsync(cancellationToken);

        return clientPipeStream;
    }

    public async Task<ResponseBase> Send(RequestBase request, CancellationToken cancellationToken = default)
    {
        var pipeStream = await TryToConnect(cancellationToken);

        var validRequest = _validator.ValidateRequest(request);

        var preparedRequest = _preparer.PrepareRequest(validRequest);

        var serializeRequest = await _serializer.SerializeRequest(preparedRequest);

        await _byteWriter.Write(serializeRequest, pipeStream);

        var deserializeResponse = await _byteReader.Read(pipeStream);

        var response = await _serializer.DeserializeResponse(deserializeResponse);

        return response;
    }
}
