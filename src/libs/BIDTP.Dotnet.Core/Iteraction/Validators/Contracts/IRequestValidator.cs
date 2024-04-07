using BIDTP.Dotnet.Core.Iteraction.Dtos;

namespace BIDTP.Dotnet.Core.Iteraction.Validators.Contracts;

public interface IRequestValidator
{
    void Validate(Request request);
}