using BIDTP.Dotnet.Core.Iteraction.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Exceptions
{
    public class BIDTPException : Exception
    {
        public InternalServerErrorType InternalServerErrorType { get; }
        public BIDTPException(string message, InternalServerErrorType internalServerErrorType)
            : base(message)
        {
            InternalServerErrorType = internalServerErrorType;
        }
    }
}
