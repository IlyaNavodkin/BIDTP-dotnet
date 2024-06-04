using Lib.Iteraction.Bytes.Contracts;
using Lib.Iteraction.Handle.Contracts;
using Lib.Iteraction.Mutation.Contracts;
using Lib.Iteraction.Serialization.Contracts;
using Lib.Iteraction.Validation.Contracts;

namespace Lib.Iteraction.Configurations
{
    public class ServerConfiguration
    {
        public IValidator Validator { get; set; }
        public IPreparer Preparer { get; set; }
        public ISerializer Serializer { get; set; }
        public IByteWriter ByteWriter { get; set; }
        public IByteReader ByteReader { get; set; }
        public IRequestHandler RequestHandler { get; set; }

        public string ServerName { get; set; }
        public string PipeName { get; set; }
    }
}
