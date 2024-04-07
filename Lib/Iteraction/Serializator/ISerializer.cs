using BIDTP.Dotnet.Core.Iteraction.Events;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Serializator;

public interface ISerializer
{
    Task<MemoryStream> SerializeRequest(RequestBase request);
    Task<RequestBase> DeserializeRequest(MemoryStream request);
    Task<MemoryStream> SerializeResponse(ResponseBase response);
    Task<ResponseBase> DeserializeResponse(MemoryStream response);
    public event EventHandler<ProgressEventArgs> ByteProgress;
}