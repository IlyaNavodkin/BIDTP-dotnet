using BIDTP.Dotnet.Core.Iteraction.Enums;

namespace BIDTP.Dotnet.Core.Iteraction.Contracts;

public abstract class TransmitionObject
{
    public Dictionary<string, string> Headers = new();
    public string Body;
    public abstract T GetBody<T>();
    public abstract void SetBody<T>(T body);
    public MessageType MessageType;
}