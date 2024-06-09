using System;
using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Events
{
    public class ResponseReceivedProgressEventArgs : EventArgs
    {
        public ResponseBase Response { get; }

        public ResponseReceivedProgressEventArgs(ResponseBase response)
        {
            Response = response;
        }
    }
}
