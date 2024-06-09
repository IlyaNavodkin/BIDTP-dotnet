using BIDTP.Dotnet.Core.Iteraction.Handle;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction.Routing.Contracts
{
    public interface IController
    {
        Task HandleRequest(string actionName, Context context);
    }
}
