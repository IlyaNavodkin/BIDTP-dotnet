using System;
using System.IO;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Events;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteReader
{
    Task<byte[]> Read(Stream stream);
}