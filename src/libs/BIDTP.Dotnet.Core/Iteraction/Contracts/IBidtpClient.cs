using BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Mutation.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Serialization.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts
{
    public interface IBidtpClient
    {
        void AddByteReader(IByteReader byteReader);
        void AddByteWriter(IByteWriter byteWriter);
        void AddLogger(ILogger logger);
        void AddPreparer(IPreparer preparer);
        void AddSerializer(ISerializer serializer);
        void AddValidator(IValidator validator);
        Task<ResponseBase> Send(RequestBase request, CancellationToken cancellationToken = default);
        void SetPipeName(string pipeName);
    }
}