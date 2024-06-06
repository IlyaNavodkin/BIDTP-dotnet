using BIDTP.Dotnet.Core.Iteraction.Handle;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts;

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
