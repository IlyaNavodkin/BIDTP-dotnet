using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BIDTP.Dotnet.Extensions;

/// <summary>
///  Extensions of the AppDomain class
/// </summary>
public static class AppDomainExtensions
{
    /// <summary>
    ///  Get the hashed pipe name of the server. Recommended for use.
    /// </summary>
    /// <returns> The hashed pipe name. </returns>
    public static string GetHashedPipeName(this AppDomain appDomain)
    {
        var sha256Managed = new SHA256Managed();
            
        var serverDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var pipeNameInput = $"{Environment.UserName}.{serverDirectory}";
            
        var hash = sha256Managed.ComputeHash(Encoding.Unicode.GetBytes(pipeNameInput));
            
        var name = Convert.ToBase64String(hash)
            .Replace("/", "_")
            .Replace("=", string.Empty);
            
        return name;
    }
}