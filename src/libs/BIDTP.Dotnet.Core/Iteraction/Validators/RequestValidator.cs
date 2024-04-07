using System;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Mutators.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Validators.Contracts;

namespace BIDTP.Dotnet.Core.Iteraction.Validators;

public class RequestValidator : IRequestValidator
{
    public void Validate(Request request)
    {
        if (request.GetBody<string>() is null) 
            throw new Exception("Request body can't be null, check the request");
        
        if (request.Headers is null) 
            throw new Exception("Request headers can't be null, check the request");
    }
}  
