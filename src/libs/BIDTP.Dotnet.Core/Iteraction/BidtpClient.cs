using BIDTP.Dotnet.Core.Iteraction.Bytes;
using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Events;
using BIDTP.Dotnet.Core.Iteraction.Mutation;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction;

public class BidtpClient : IBidtpClient
{
    public IValidator Validator;
    public IPreparer Preparer;
    public ISerializer Serializer;
    public IByteWriter ByteWriter;
    public IByteReader ByteReader;

    public string Pipename = "DefaultPipeName";
    public int Timeout = 5000;

    public event EventHandler<EventArgs> RequestSended;
    public event EventHandler<EventArgs> ResponseReceived;
    public event EventHandler<EventArgs> IsConnected;

    public BidtpClient()
    {
        Validator = new Validator();
        Preparer = new Preparer();
        Serializer = new Serializer();
        ByteWriter = new ByteWriter();
        ByteReader = new ByteReader();
    }

    public BidtpClient(IValidator validator, 
        IPreparer preparer, ISerializer serializer, 
        IByteWriter byteWriter, IByteReader byteReader,
        string pipeName)
    {
        Validator = validator;
        Preparer = preparer;
        Serializer = serializer;
        ByteWriter = byteWriter;
        ByteReader = byteReader;

        Pipename = pipeName;
    }

    private async Task<NamedPipeClientStream> TryToConnect(CancellationToken cancellationToken = default)
    {
        var clientPipeStream = new NamedPipeClientStream
        (".", Pipename, PipeDirection.InOut, PipeOptions.Asynchronous);

        await clientPipeStream.ConnectAsync(Timeout, cancellationToken);

        return clientPipeStream;
    }

    public async Task<ResponseBase> Send(RequestBase request, CancellationToken cancellationToken = default)
    {
        var pipeStream = await TryToConnect(cancellationToken);

        IsConnected?.Invoke(this, new ClientConnectedEventArgs(Pipename));

        var validRequest = Validator.ValidateRequest(request);

        var preparedRequest = Preparer.PrepareRequest(validRequest);

        var serializeRequest = await Serializer.SerializeRequest(preparedRequest);

        await ByteWriter.Write(serializeRequest, pipeStream);

        RequestSended?.Invoke(this, new RequestSendedProgressEventArgs(preparedRequest));

        var deserializeResponse = await ByteReader.Read(pipeStream);

        var response = await Serializer.DeserializeResponse(deserializeResponse);

        ResponseReceived?.Invoke(this, new ResponseReceivedProgressEventArgs(response));

        return response;
    }
}
