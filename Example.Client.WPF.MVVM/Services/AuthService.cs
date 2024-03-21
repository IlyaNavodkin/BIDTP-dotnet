using System;
using System.Net.Http;

namespace Example.Client.WPF.MVVM.Services;

public class AuthService
{
    private static readonly Lazy<AuthService> Instance = 
        new Lazy<AuthService>(() => new AuthService());
    
    private string _authToken = string.Empty;
    
    public static AuthService GetInstance () => Instance.Value;
    
    public void SetAuthToken(string authToken) => _authToken = authToken;
    public string GetAuthToken() => _authToken;
}