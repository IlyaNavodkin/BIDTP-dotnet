﻿using System.Text.Encodings.Web;
using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Newtonsoft.Json;

namespace Example.Server.Providers;

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
            var error = new Error
            {
                ErrorCode = 140,
                Message = "Вы не авторизованы!",
                Description = "Вы не авторизованы!"
            };
            
            var jsonString = JsonConvert.SerializeObject(error);
            
            context.Response = new Response(StatusCode.Unauthorized)
            {
                Body = jsonString
            };
        }
        
        return authorizationIsValid;
    }
}