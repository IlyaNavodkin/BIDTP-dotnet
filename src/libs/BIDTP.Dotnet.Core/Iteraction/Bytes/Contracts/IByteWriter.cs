using System;
using System.IO;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Events;

namespace BIDTP.Dotnet.Core.Iteraction.Bytes.Contracts;

public interface IByteWriter
{
    Task Write(byte[] stream, Stream streamToWrite);
}