using System;
using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Events
{
    public class RequestReceivedProgressEventArgs : EventArgs
    {
        public RequestBase Request { get; }

        public RequestReceivedProgressEventArgs(RequestBase request)
        {
            Request = request;
        }
    }
}
