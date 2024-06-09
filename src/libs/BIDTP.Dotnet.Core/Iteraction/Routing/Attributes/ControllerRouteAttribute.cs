using System;

namespace BIDTP.Dotnet.Core.Iteraction.Routing.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ControllerRouteAttribute : Attribute
    {
        public string Route { get; }

        public ControllerRouteAttribute(string route)
        {
            Route = route;
        }
    }
}
