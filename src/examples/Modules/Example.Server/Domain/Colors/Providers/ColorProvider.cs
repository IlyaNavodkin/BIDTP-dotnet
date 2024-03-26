namespace Example.Server.Domain.Colors.Providers;

public class ColorProvider 
{
    public Task<string> GetRandomColorString()
    {
        var random = new Random();
        
        return Task.FromResult($"New color => #{random.Next(0x1000000):X6}");
    }
}