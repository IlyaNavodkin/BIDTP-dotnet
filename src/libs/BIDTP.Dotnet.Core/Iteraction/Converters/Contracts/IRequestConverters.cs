using BIDTP.Dotnet.Core.Iteraction.Dtos;

namespace BIDTP.Dotnet.Core.Iteraction.Converters.Contracts;

public interface IByteWriter
{
    void WriteRequest(Request request);
}