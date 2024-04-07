using BIDTP.Dotnet.Core.Iteraction.Dtos;

namespace BIDTP.Dotnet.Core.Iteraction.Mutators.Contracts;

public interface IRequestMutator
{
    void SetGeneralHeaders(Request request);
}