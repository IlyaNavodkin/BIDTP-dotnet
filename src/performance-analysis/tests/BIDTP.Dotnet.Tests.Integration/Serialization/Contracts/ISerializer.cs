using Lib.Iteraction.Contracts;

namespace Lib.Iteraction.Serialization.Contracts;

public interface ISerializer
{
    Task<byte[]> SerializeRequest(RequestBase request);
    Task<RequestBase> DeserializeRequest(byte[] request);

    Task<byte[]> SerializeResponse(ResponseBase response);
    Task<ResponseBase> DeserializeResponse(byte[] response);
}