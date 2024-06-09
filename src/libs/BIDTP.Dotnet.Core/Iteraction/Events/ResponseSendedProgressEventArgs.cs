using System;
using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Events
{
    public class ResponseSendedProgressEventArgs : EventArgs
    {
        public ResponseBase Response { get; }

        public ResponseSendedProgressEventArgs(ResponseBase response)
        {
            Response = response;
        }
    }
}
