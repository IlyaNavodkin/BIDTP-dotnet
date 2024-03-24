using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Providers;

namespace BIDTP.Dotnet.Core.Iteraction.Interfaces;

/// <summary>
///  Pre-invokable method of the server 
/// </summary>
public interface IMethodScopedPreInvokable
{
    /// <summary>
    ///  Pre-invokable method of the server
    /// </summary>
    /// <param name="context"> The context </param>
    /// <returns> The task. </returns>
    Task Invoke(Context context);
}