using BIDTP.Dotnet.Core.Iteraction.Events;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Serializator;

public interface ISerializer
{
    Task<byte[]> SerializeRequest(RequestBase request);
    Task<RequestBase> DeserializeRequest(byte[] request);

    Task<byte[]> SerializeResponse(ResponseBase response);
    Task<ResponseBase> DeserializeResponse(byte[] response);

    public event EventHandler<ProgressEventArgs> ByteProgress;
}