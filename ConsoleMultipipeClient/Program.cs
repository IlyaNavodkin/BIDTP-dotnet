using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace ConsoleMultipipeClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var tasks = new Task[8];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    Random rnd = new Random();
                    int delay = rnd.Next(1000, 3001); // Генерируем случайную задержку от 1000 до 3000 миллисекунд
                    await Task.Delay(delay);
                    await ConnectToServer();
                });
            }
            
            await Task.WhenAll(tasks);
            
            // await ConnectToServer();
        }

        static async Task ConnectToServer()
        {
            using (var pipeClient = new NamedPipeClientStream(".", "pipe_name", PipeDirection.InOut))
            {
                var guidName = Guid.NewGuid().ToString();
                
                Console.WriteLine("Подключение к серверу...");
                await pipeClient.ConnectAsync();
                
                var binaryReader = new BinaryReader(pipeClient);
                var binaryWriter = new BinaryWriter(pipeClient);
            
                binaryWriter.Write(guidName);
                
                var message = binaryReader.ReadString();
                
                Console.WriteLine($"Сообщение от сервера: {message}");
            }
        }
    }
}