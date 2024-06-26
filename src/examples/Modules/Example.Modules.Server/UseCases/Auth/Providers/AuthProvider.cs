﻿using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Schema;

namespace Example.Modules.Server.Domain.Auth.Providers;

/// <summary>
///  The auth provider for the server
/// </summary>
public class AuthProvider
{
    private readonly string _validToken = "123";    
    /// <summary>
    ///  Is valid token
    /// </summary>
    /// <param name="token"> The token. </param>
    /// <returns> The task. </returns>
    public Task<bool> IsValid(string token)
    {
        return Task.FromResult(token == _validToken);
    }

    /// <summary>
    ///  Is auth valid for the request
    /// </summary>
    /// <param name="context"> The context. </param>
    /// <returns> The task. </returns>
    public async Task<bool>IsAuth(Context context)
    {
        var request = context.Request;

        request.Headers.TryGetValue("Authorization", out var authorization);
        var authorizationIsValid = await IsValid(authorization);
    
        if(!authorizationIsValid)
        {
            var error = new BIDTPError
            {
                ErrorCode = 140,
                Message = "Вы не авторизованы!",
                Description = "Вы не авторизованы!"
            };
            
            var response = new Response(StatusCode.Unauthorized);
            
            response.SetBody<BIDTPError>(error);

            context.Response = response;
        }
        
        return authorizationIsValid;
    }
}