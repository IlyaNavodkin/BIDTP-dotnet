namespace BIDTP.Dotnet.Core.Iteraction.Contracts;

public abstract class RequestBase : TransmitionObject
{
    public void SetRoute(string route)
    {
        Headers.Add("Route", route);
    }
}