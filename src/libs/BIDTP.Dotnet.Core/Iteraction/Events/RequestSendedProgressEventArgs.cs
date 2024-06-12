using System;
using BIDTP.Dotnet.Core.Iteraction.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction
{
    public class RequestSendedProgressEventArgs : EventArgs
    {
        public RequestBase Request { get; }

        public RequestSendedProgressEventArgs(RequestBase request)
        {
            Request = request;
        }
    }
}
