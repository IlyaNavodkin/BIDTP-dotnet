using System.Threading.Tasks;

namespace ConsoleMultiPipeServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serverManager = new PipeServerManager(4);
            await serverManager.Start();
            
            serverManager.Listen(); 
        }
    }
}