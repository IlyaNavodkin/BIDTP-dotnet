using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;
using Microsoft.Extensions.Logging;

namespace Lib.Iteraction.Contracts
{
    public interface IClientBase
    {
        void AddByteReader(IByteReader byteReader);
        void AddByteWriter(IByteWriter byteWriter);
        void AddLogger(ILogger logger);
        void AddPreparer(IPreparer preparer);
        void AddSerializer(ISerializer serializer);
        void AddValidator(IValidator validator);
        Task<ResponseBase> Send(RequestBase request);
        void SetPipeName(string pipeName);
    }
}