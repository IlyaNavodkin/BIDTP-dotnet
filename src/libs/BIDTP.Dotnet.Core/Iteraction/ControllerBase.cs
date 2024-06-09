using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BIDTP.Dotnet.Core.Iteraction
{
    public abstract class ControllerBase : IController
    {
        public async Task HandleRequest(string actionName, Context context)
        {
            var typeController = GetType();
            var methods = typeController.GetMethods();
            var routeMethod = methods.FirstOrDefault(m => m.Name == actionName);

            if (routeMethod != null)
            {
                var attributes = routeMethod.GetCustomAttributes(true)
                    .Where(attr => attr is IMethodScopedPreInvokable)
                    .Cast<IMethodScopedPreInvokable>()
                    .ToArray();

                foreach (var attribute in attributes)
                {
                    await attribute.Invoke(context);
                    if (context.Response != null) return;
                }

                await (Task)routeMethod.Invoke(this, new object[] { context });
            }
            else
            {
                throw new Exception($"No action found for route: {actionName}");
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ControllerRouteAttribute : Attribute
    {
        public string Route { get; }

        public ControllerRouteAttribute(string route)
        {
            Route = route;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RouteAttribute : Attribute
    {
        public string Route { get; }

        public RouteAttribute(string route)
        {
            Route = route;
        }
    }

    public interface IController
    {
        Task HandleRequest(string actionName, Context context);
    }
}
