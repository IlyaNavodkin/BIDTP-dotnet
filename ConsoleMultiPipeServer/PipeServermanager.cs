using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMultiPipeServer
{
class PipeServerManager
{
    private ConcurrentQueue<NamedPipeServerStream> _availablePipes;
    private readonly int _maxPipes;

    public PipeServerManager(int maxPipes)
    {
        _maxPipes = maxPipes;
        _availablePipes = new ConcurrentQueue<NamedPipeServerStream>();
    }

    public async Task Start()
    {
        for (int i = 0; i < _maxPipes; i++)
        {
            await CreateNewPipeAsync();
        }
    }

    public void Listen()
    {
        while (true)
        {
            _availablePipes.TryDequeue(out var result);
            
            if (result == null) continue;
            
            Console.WriteLine("Свободный pipe готов к работе.");
            
            Task.Run(() => HandleClient(result)); // Обрабатываем клиента в отдельном потоке
        }
    }
    

    private async Task CreateNewPipeAsync()
    {
        var pipe =
            new NamedPipeServerStream(
                "pipe_name", PipeDirection.InOut, _maxPipes,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        
        _availablePipes.Enqueue(pipe);
    }

    private async Task HandleClient(NamedPipeServerStream pipe)
    {
        try
        {
            await pipe.WaitForConnectionAsync();
            
            var binaryReader = new BinaryReader(pipe);
            var binaryWriter = new BinaryWriter(pipe);

            var request = binaryReader.ReadString();
            
            binaryWriter.Write($"Сообщение от клиента: {request}");
        }
        catch (IOException)
        {
            Console.WriteLine("Клиент отключился.");
        }
        finally
        {
            pipe.Close();
            await CreateNewPipeAsync();
        }
    }
}} 