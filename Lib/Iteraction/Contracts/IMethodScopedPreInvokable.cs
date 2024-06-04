using Lib.Iteraction.Handle;

namespace Lib.Iteraction.Contracts;

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
