using System.IO;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteReader
{
    Task<byte[]> Read(Stream stream);
}