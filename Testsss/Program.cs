using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Enums;

namespace Testsss
{
    internal class Program
    {
        private const int ChunkSize = 1024;
        
    
    static async Task Main(string[] args)
    {
        var pipeStreamA = new NamedPipeServerStream("mypipe1", PipeDirection.InOut, 2);
        var pipeStreamB = new NamedPipeServerStream("mypipe1", PipeDirection.InOut, 2);
        
        // Подключаемся к серверу асинхронно с тремя клиентами
        // Task client1 = ConnectToServer("Client 1");
        // Task client2 = ConnectToServer("Client 2");
        // Task client3 = ConnectToServer("Client 3");
        //
        // // Ожидаем завершения всех клиентских задач
        // await Task.WhenAll(client1, client2, client3);
    }

    static async Task StartServer()
    {
        // Создаем именованный канал
        var pipeServer = new NamedPipeServerStream("my_pipe", PipeDirection.InOut,
           1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        var pipeServer2 = new NamedPipeServerStream("my_pipe", PipeDirection.InOut,
           1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        Console.WriteLine("Ожидание подключения клиентов...");


    }

    static async Task ConnectToServer(string clientName)
    {
        using (var pipeClient = new NamedPipeClientStream(".", "my_pipe", PipeDirection.InOut))
        {
            Console.WriteLine("Подключение к серверу...");
            await pipeClient.ConnectAsync();

            Console.WriteLine($"Клиент {clientName} подключен.");

            // Отправка и получение данных с сервера
            using (var reader = new StreamReader(pipeClient))
            using (var writer = new StreamWriter(pipeClient))
            {
                // Отправляем сообщения серверу и читаем ответы
                for (int i = 0; i < 5; i++)
                {
                    // Отправляем сообщение серверу
                    await writer.WriteLineAsync($"{clientName}: Message {i}");
                    await writer.FlushAsync();

                    // Читаем ответ от сервера
                    string response = await reader.ReadLineAsync();
                    Console.WriteLine($"Ответ от сервера для {clientName}: {response}");
                }
            }
        }
    }

    static async Task HandleClient(NamedPipeServerStream pipeServer)
    {
        try
        {
            using (var reader = new StreamReader(pipeServer))
            using (var writer = new StreamWriter(pipeServer))
            {
                while (true)
                {
                    // Чтение данных от клиента
                    string request = await reader.ReadLineAsync();
                    if (request == null)
                        break; // Клиент отключился

                    Console.WriteLine("Получено сообщение от клиента: " + request);

                    // Отправка ответа клиенту
                    string response = "Сообщение получено: " + request;
                    await writer.WriteLineAsync(response);
                    await writer.FlushAsync();
                }
            }
        }
        catch (IOException)
        {
            // Клиент отключился
            Console.WriteLine("Клиент отключился.");
        }
        finally
        {
            // Закрытие канала
            pipeServer.Close();
        }
    }

        // public static void Main(string[] args)
        // {
        //     var dictionary = new Dictionary<string, string>();
        //     
        //     dictionary.Add(nameof(MessageType), MessageType.General.ToString());
        //     dictionary.Add("Headers", "Папа123");
        //     // dictionary.Add("Body", "Мама228");
        //     
        //     
        //     try
        //     {
        //         dictionary.Add("Headers", "ewdwewe");
        //     }
        //     catch (Exception e)
        //     {
        //         dictionary.Add("Body", e.StackTrace);
        //     }
        //     
        //     var encoding = Encoding.UTF8;
        //     var decoding = Encoding.UTF8;
        //     
        //     using (var memoryStream = new MemoryStream())
        //     {
        //         var binaryWriter = new BinaryWriter(memoryStream, encoding);
        //     
        //         var messageType = (MessageType)Enum.Parse(typeof(MessageType), dictionary[nameof(MessageType)]);
        //         binaryWriter.Write((int)messageType);
        //     
        //         Console.WriteLine($"==========[ЗАПИСЬ БАЙТОВ]========");
        //     
        //         Console.WriteLine($"Тип сообщения|{messageType.ToString()}");
        //     
        //         if (messageType == MessageType.General)
        //         {
        //             var headerString = dictionary["Headers"];
        //             var bodyString = dictionary["Body"];
        //     
        //             var headerBuffer = encoding.GetBytes(headerString);
        //             var bodyBuffer = encoding.GetBytes(bodyString);
        //             
        //             binaryWriter.Write(headerBuffer.Length);
        //             Console.WriteLine($"Header length|{headerBuffer.Length}");
        //     
        //             binaryWriter.Write(headerBuffer);
        //             Console.WriteLine($"Header string|{headerString}");
        //     
        //             binaryWriter.Write(bodyBuffer.Length);
        //             Console.WriteLine($"Body length|{bodyBuffer.Length}");
        //     
        //             binaryWriter.Write(bodyBuffer);
        //             Console.WriteLine($"Body string|{bodyString}");
        //         }
        //     
        //         var buffer = memoryStream.ToArray();
        //     
        //         var allLength = buffer.Length;
        //         var allLengthBuffer = BitConverter.GetBytes(allLength);
        //     
        //         buffer = allLengthBuffer.Concat(buffer).ToArray();
        //     
        //         Console.WriteLine($"==========[ЧТЕНИЕ ИЗ БАЙТОВ]========");
        //     
        //         using (var memoryStream1 = new MemoryStream(buffer))
        //         using (var binaryReader = new BinaryReader(memoryStream1, decoding))
        //         {
        //             var allLengthFromReader = binaryReader.ReadInt32(); // Считываем общую длину
        //             var messageTypeFromReader = (MessageType)binaryReader.ReadInt32();
        //     
        //             Console.WriteLine($"Тип сообщения|{messageTypeFromReader.ToString()}");
        //     
        //             var headerContentLengthFromReader = binaryReader.ReadInt32(); // Считываем длину заголовка
        //             Console.WriteLine($"Header length|{headerContentLengthFromReader.ToString()}");
        //             
        //             var headerBufferFromReader = binaryReader.ReadBytes(headerContentLengthFromReader); // Читаем байты заголовка
        //             var headerFromFromReader = decoding.GetString(headerBufferFromReader); // Декодируем в строку
        //             
        //             Console.WriteLine($"Header string|{headerFromFromReader}");
        //     
        //             var bodyLengthFromReader = binaryReader.ReadInt32(); // Считываем длину тела
        //             Console.WriteLine($"Body length|{bodyLengthFromReader.ToString()}");
        //             
        //             var bodyBufferFromReader = binaryReader.ReadBytes(bodyLengthFromReader); // Читаем байты тела
        //             var bodyFromReader = decoding.GetString(bodyBufferFromReader); // Декодируем в строку
        //             
        //             Console.WriteLine($"Body string|{bodyFromReader}");
        //         }
        //     }        
        // }
        
        private static async Task WriteServer(Dictionary<string, string> dictionary, NamedPipeServerStream _serverPipeStream, CancellationToken cancellationToken)
        {
            var bytesWrite = 0;

            using (var memoryStream = new MemoryStream())
            {
                var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8);
            
                var messageType = (MessageType)Enum.Parse(typeof(MessageType), dictionary[nameof(MessageType)]);
            
                binaryWriter.Write((int)messageType);
            
                if (messageType == MessageType.General)
                {
                    var body = dictionary["Body"];
                    var bodyBytes = Encoding.UTF8.GetBytes(body);
            
                    var headers = dictionary["Headers"];
                    var headersBytes = Encoding.UTF8.GetBytes(headers);
                
                    binaryWriter.Write(headersBytes.Length);
                    binaryWriter.Write(headersBytes);
                
                    binaryWriter.Write(bodyBytes.Length);
                    binaryWriter.Write(body);
                }

                var buffer = memoryStream.ToArray();
                var allLength = buffer.Length;
                var allLengthBuffer = BitConverter.GetBytes(allLength);
            
                buffer = allLengthBuffer.Concat(buffer).ToArray();
            
                await _serverPipeStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            }
        }

        private static async Task<Dictionary<string,string>> ReadServer(NamedPipeServerStream _serverPipeStream, CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, string>();
        
            var messageLengthBytes = new byte[4];
            await _serverPipeStream.ReadAsync(messageLengthBytes, 0, messageLengthBytes.Length);
            var messageLength = BitConverter.ToInt32(messageLengthBytes, 0);
        
            var contentBuffer = new byte[messageLength];
            await _serverPipeStream.ReadAsync(contentBuffer, 0, contentBuffer.Length);
        
            using (var memoryStream = new MemoryStream(contentBuffer))
            {
                var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);

                var messageType = (MessageType)binaryReader.ReadInt32( );
            
                result.Add(nameof(MessageType), messageType.ToString());
            
                if(messageType == MessageType.HealthCheck)
                {
                    return result;
                }
            
                var headersLength = binaryReader.ReadInt32();
                var headers = binaryReader.ReadChars(headersLength);
                var headersString = new string(headers);
            
                var bodyLength = binaryReader.ReadInt32();
                var body = binaryReader.ReadChars( bodyLength );
                var bodyString = new string(body);
            
                result.Add("Headers", headersString);
                result.Add("Body", bodyString);
                
                return result;
            } 
        }
        
        private static async Task WriteCLient(Dictionary<string, string> dictionary, 
            NamedPipeClientStream _clientPipeStream, CancellationToken cancellationToken)
        {
            var bytesWrite = 0;

            using (var memoryStream = new MemoryStream())
            {
                var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8);
            
                var messageType = (MessageType)Enum.Parse(typeof(MessageType), dictionary[nameof(MessageType)]);
                binaryWriter.Write((int)messageType);
            
                if (messageType == MessageType.General)
                {
                    var headerString = dictionary["Headers"];
                    var body = dictionary["Body"];

                    var headerBuffer = Encoding.UTF8.GetBytes(headerString);
                    var bodyBuffer = Encoding.UTF8.GetBytes(body);
                
                    binaryWriter.Write(headerBuffer.Length);
                    binaryWriter.Write(headerString);
                
                    binaryWriter.Write(bodyBuffer.Length);
                    binaryWriter.Write(body);
                    
                }
        
                var buffer = memoryStream.ToArray();
            
                var allLength = buffer.Length;
                var allLengthBuffer = BitConverter.GetBytes(allLength);
            
                buffer = allLengthBuffer.Concat(buffer).ToArray();
            
                var memoryStream1 = new MemoryStream(buffer);
                var binaryReader = new BinaryReader(memoryStream1, Encoding.UTF8);
                
                var lenght = binaryReader.ReadInt32();
                var messl1 = (MessageType) binaryReader.ReadInt32();
                var l1 = binaryReader.ReadInt32();
                var l2 = new string(binaryReader.ReadChars(l1));
                
                var l3 = binaryReader.ReadInt32();
                var l4 = new string(binaryReader.ReadChars(l3));
                
                await _clientPipeStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                
            }
        }
        private static async Task<Dictionary<string,string>> ReadClient( NamedPipeClientStream _clientPipeStream, CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, string>();
        
            var messageLengthBuffer = new byte[4];
            await _clientPipeStream.ReadAsync(messageLengthBuffer, 0, messageLengthBuffer.Length, cancellationToken);
            var messageLength = BitConverter.ToInt32(messageLengthBuffer, 0);
        
            var contentBuffer = new byte[messageLength];
            await _clientPipeStream.ReadAsync(contentBuffer, 0, contentBuffer.Length, cancellationToken);
        
            using (var memoryStream = new MemoryStream(contentBuffer))
            {
                var binaryReader = new BinaryReader(memoryStream);
            
                var messageTypeBuffer = (MessageType) binaryReader.ReadInt32();
            
                result.Add(nameof(MessageType), messageTypeBuffer.ToString());
        
                if(messageTypeBuffer == MessageType.HealthCheck)
                {
                    return result;
                }
            
                var headersLength = binaryReader.ReadInt32();
                var headers = binaryReader.ReadChars(headersLength);
                var headersString = new string(headers);
            
                result.Add("Headers",headersString);
            
                var bodyLenght = binaryReader.ReadInt32();
                var readChars =   binaryReader.ReadChars( bodyLenght );
                var body = new string(readChars);
        
                result.Add("Body",body);
            }
        
            return result;
        }


        public static async Task RunPipeServerAndClient(string message)
        {
            // Создание и запуск сервера
            Task serverTask = Task.Run(async () =>
            {
                using (NamedPipeServerStream serverPipe = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 
                           2, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                {
                    Console.WriteLine("Сервер: Ожидание подключения клиента...");
                    serverPipe.WaitForConnection();
                    Console.WriteLine("Сервер: Клиент подключен.");
                    
                    
                    var dictionary = await ReadServer(serverPipe, CancellationToken.None);
                    
                    Console.WriteLine(dictionary["Headers"]);
                    Console.WriteLine(dictionary["Body"]);
                    
                    await WriteServer(dictionary, serverPipe, CancellationToken.None);
                    
                    // Закрытие серверного пайпа
                    serverPipe.Close();
                }
            });

            // Создание и подключение клиента
            using (NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", "testpipe", 
                       PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                Console.WriteLine("Клиент: Попытка подключения к серверу...");
                await clientPipe.ConnectAsync();
                Console.WriteLine("Клиент: Подключение к серверу установлено.");

                var dictionary = new Dictionary<string, string>();
                
                dictionary.Add(nameof(MessageType), MessageType.General.ToString());
                dictionary.Add("Headers", "Hello");
                dictionary.Add("Body", "World");
                
                await WriteCLient(dictionary, clientPipe, CancellationToken.None);
                
                var result = await ReadClient(clientPipe, CancellationToken.None);
                
                Console.WriteLine(result["Headers"]);
                Console.WriteLine(result["Body"]);
                
                
                // // Отправка сообщения серверу
                // byte[] messageBytes = Encoding.Unicode.GetBytes(message);
                // clientPipe.Write(messageBytes, 0, messageBytes.Length);
                // Console.WriteLine($"Клиент: Отправлено сообщение серверу: {message}");
                //
                // // Получение ответа от сервера
                // byte[] responseBuffer = new byte[256];
                // int responseBytesRead = clientPipe.Read(responseBuffer, 0, responseBuffer.Length);
                // string serverResponse = Encoding.Unicode.GetString(responseBuffer, 0, responseBytesRead);
                // Console.WriteLine($"Клиент: Получен ответ от сервера: {serverResponse}");

                // Закрытие клиентского пайпа
                clientPipe.Close();
            }

            // Ожидание завершения работы сервера
            await serverTask;
        }

    }
}