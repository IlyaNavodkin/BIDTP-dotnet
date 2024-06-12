using System;

namespace BIDTP.Dotnet.Core.Iteraction.Routing.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodRouteAttribute : Attribute
    {
        public string Route { get; }

        public MethodRouteAttribute(string route)
        {
            Route = route;
        }
    }
}
