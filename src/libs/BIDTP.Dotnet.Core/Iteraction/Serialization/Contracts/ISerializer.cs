using BIDTP.Dotnet.Core.Iteraction.Contracts;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;

public interface ISerializer
{
    Task<byte[]> SerializeRequest(RequestBase request);
    Task<RequestBase> DeserializeRequest(byte[] request);

    Task<byte[]> SerializeResponse(ResponseBase response);
    Task<ResponseBase> DeserializeResponse(byte[] response);
}