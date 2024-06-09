using System;
using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Events
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public string PipeName { get; }

        public ClientConnectedEventArgs(string pipeName)
        {
            PipeName = pipeName;
        }
    }
}
