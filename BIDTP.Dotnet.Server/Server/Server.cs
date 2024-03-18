using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Piders.Dotnet.Core;
using Piders.Dotnet.Core.Constants;
using Piders.Dotnet.Core.Request;
using Piders.Dotnet.Core.Response;
using Piders.Dotnet.Core.Response.Dtos;
using Piders.Dotnet.Core.Response.Enums;
using Piders.Dotnet.Server.Server.Iteraction;

namespace Piders.Dotnet.Server.Server;

/// <summary>
///  Class of the server. 
/// </summary>
public class Server : IHost
{
    private readonly Dictionary<string, Func<Context, Task>[]> _routeHandlers;
    private readonly ConcurrentDictionary<string, IHostedService> _workers;
    private readonly string _pipeName;
    private readonly SemaphoreSlim _streamSemaphore;
    
    private NamedPipeServerStream _serverPipeStream;
    
    /// <summary>
    ///  The chunk size for the transmission data
    /// </summary>
    public int ChunkSize { get; set; }
    private int ReconnectTimeRate { get; }
    /// <summary>
    ///  Server of the SIDTP protocol
    /// </summary>
    /// <param name="pipeName"> The name of the pipe </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    /// <param name="routeHandlers"> The route handlers </param>
    /// <param name="buildServiceProvider"> The service provider </param>
    public Server(string pipeName, int chunkSize, int reconnectTimeRate,
        Dictionary<string, Func<Context, Task>[]> routeHandlers, ServiceProvider buildServiceProvider)
    {
        _pipeName = pipeName;
        ChunkSize = chunkSize;
        ReconnectTimeRate = reconnectTimeRate;

        _routeHandlers = routeHandlers;
        Services = buildServiceProvider;
        
        _workers = new ConcurrentDictionary<string, IHostedService>(); 
        
        _streamSemaphore  = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    ///  Get the name of the pipe 
    /// </summary>
    /// <returns> The name of the pipe </returns>
    public string GetPipeName() => _pipeName;
    
    /// <summary>
    ///  Start the server 
    /// </summary>
    /// <param name="cancellationToken"> The cancellation token. </param>
    /// <exception cref="Exception"> The stream already created. </exception>
    public async Task StartAsync(CancellationToken  cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Debug.WriteLine("[Start Async]: Startup.");
                
                if(_serverPipeStream is not null) throw new Exception("Stream already created");
                
                _serverPipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 
                    1, PipeTransmissionMode.Message);
                await _serverPipeStream.WaitForConnectionAsync(cancellationToken);
                
                Debug.WriteLine("[Start Async]: Client connected.");
                
                await Listen(cancellationToken);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[Start Async]: {exception}");
                DisposeStreams();
            }
            
            await Task.Delay(ReconnectTimeRate); 
        }
    }

    /// <summary>
    ///  Stop the server and dispose resources
    /// </summary>
    /// <param name="cancellationToken"> The cancellation token. </param>
    /// <returns> The task. </returns>
    public Task StopAsync(CancellationToken cancellationToken = new())
    {
        Dispose();
        
        return Task.CompletedTask;
    }

    /// <summary>
    ///  Add a background service to the server
    /// </summary>
    /// <param name="workerName"> The name of the worker. </param>
    /// <typeparam name="T"> The type of the worker. </typeparam>
    public void AddBackgroundService<T>(string workerName = null) where T : BackgroundService
    {
        var backgroundServiceInstance = ActivatorUtilities.CreateInstance<T>(Services);
        
        workerName ??= typeof(T).Name;
        
        if (_workers.TryAdd(workerName, backgroundServiceInstance))
        {
            backgroundServiceInstance.StartAsync(CancellationToken.None);
        }
    }
    
    /// <summary>
    ///  Stop a background service from the server
    /// </summary>
    /// <param name="workerName"> The name of the worker. </param>
    /// <typeparam name="T"> The type of the worker. </typeparam>
    public void StopBackgroundService<T>(string workerName) where T : BackgroundService
    {
        workerName ??= typeof(T).Name;
        
        if (_workers.TryRemove(workerName, out var worker))
        {
            worker.StopAsync(CancellationToken.None);
        }
    }
    
    /// <summary>
    ///  Get all background workers from the server
    /// </summary>
    /// <returns> The background workers. </returns>
    public ConcurrentDictionary<string,IHostedService> GetBackgroundWorkers() => _workers;
    
    private async Task Listen(CancellationToken cancellationToken)
    {
        while (_serverPipeStream.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _streamSemaphore.WaitAsync(cancellationToken);
 
                var dictionary = await ReadAsyncInternal(cancellationToken);

                var result = new Dictionary<string,string>();
                
                dictionary.TryGetValue("MessageType", out var messageTypeString);
                
                if(messageTypeString is null) throw new Exception("Message type not found");
                
                var messageType = (MessageType) Enum.Parse(typeof(MessageType), messageTypeString);

                if (messageType is MessageType.HealthCheck)
                {
                    result.Add("MessageType", MessageType.HealthCheck.ToString());
                }
                else
                {
                    result = await ServeRequest(dictionary);
                }

                await WriteAsyncInternal(result, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[Listen]: Operation canceled");
                throw;
            }
            catch (EndOfStreamException endOfStreamException)
            {
                Debug.WriteLine($"[Listen]: {endOfStreamException}");
                throw;
            }
            catch (IOException ioException)
            {
                Debug.WriteLine($"[Listen]: {ioException}");
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[Listen]: {exception}");
                throw;
            }
            finally
            {
                _streamSemaphore.Release();
            }
        }
    }
    
    private async Task<Dictionary<string,string>> ServeRequest(Dictionary<string,string> requestMessage)
    {
        try
        {
            var body = requestMessage["Body"];
            
            var headersString = requestMessage["Headers"];
            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString);
            
            var request = new Request
            {
                Body = body,
                Headers = headers
            };
            
            request.Headers.TryGetValue(Constants.RouteHeaderName, out var route);
            
            if(route is null) throw new Exception("Route not found");
            
            var serverRouteNotExist = !_routeHandlers.TryGetValue(route, out var handlers);
        
            if (serverRouteNotExist)
            {
                var error = new Error
                {
                    Message = $"Route {route} is not found!",
                    ErrorCode = (int) InternalServerErrorType.RouteNotFoundError
                };
        
                var response = new Response(StatusCode.NotFound)
                {
                    Body = JsonConvert.SerializeObject(error)
                };
                
                SetGeneralHeaders(response);

                return ConvertToDictionary(response);
            }
        
            using (var scope = Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var context = new Context(request, serviceProvider);
        
                foreach (var handler in handlers)
                {
                    await handler(context);
                    if (context.Response != null) break;
                }
        
                if (context.Response is null) throw new InvalidOperationException("Server response is not set!");
                
                var response = context.Response;
                SetGeneralHeaders(response);
                
                var result = ConvertToDictionary(response);
                
                return result;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        
            var error = new Error 
            {
                Message = "Внутренняя ошибка сервера!",
                Description = exception.Message,
                ErrorCode = (int) InternalServerErrorType.DispatcherExceptionError
            };
        
            var response = new Response(StatusCode.ServerError)
            {
                Body = JsonConvert.SerializeObject(error)
            };
            
            SetGeneralHeaders(response);
                
            var result = ConvertToDictionary(response);
                
            return result;
        }
    }

    private static Dictionary<string, string> ConvertToDictionary(Response response)
    {
        var dictionary = new Dictionary<string, string>();
                
        var headerString = JsonConvert.SerializeObject(response.Headers);
        var bodyString = response.Body;
                
        dictionary.Add("MessageType", response.MessageType.ToString());
        dictionary.Add("StatusCode", response.StatusCode.ToString());
        dictionary.Add("Headers", headerString);
        dictionary.Add("Body", bodyString);
                
        return dictionary;
    }

    private async Task<Dictionary<string,string>> ReadAsyncInternal(CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();
        
        var bytesReads = 0;
        
        var messageLengthBuffer = new byte[4];
        await _serverPipeStream.ReadAsync(messageLengthBuffer, bytesReads, messageLengthBuffer.Length);
        var messageLength = BitConverter.ToInt32(messageLengthBuffer, 0);
        
        bytesReads += messageLengthBuffer.Length;

        var contentBuffer = new byte[messageLength];
        await _serverPipeStream.ReadAsync(contentBuffer, 0, contentBuffer.Length, cancellationToken);
        
        using (var memoryStream = new MemoryStream(contentBuffer))
        {
            var messageType = new byte[4];
            await memoryStream.ReadAsync(messageType, 0, messageType.Length, cancellationToken);
            var messageTypeValue = (MessageType) BitConverter.ToInt32(messageType, 0);
            
            result.Add("MessageType", messageTypeValue.ToString());
            
            if(messageTypeValue == MessageType.HealthCheck)
            {
                return result;
            }
            
            var totalBytes = messageLengthBuffer.Length + messageLength;
            
            bytesReads += messageType.Length;
            OnReadProgressChanged(bytesReads, totalBytes);
            
            var headersLengthBuffer = new byte[4];
            await memoryStream.ReadAsync(headersLengthBuffer, 0, headersLengthBuffer.Length, cancellationToken);
            var headersLength = BitConverter.ToInt32(headersLengthBuffer, 0);
                
            bytesReads += headersLengthBuffer.Length;
            OnReadProgressChanged(bytesReads, totalBytes);

                
            var headersBuffer = new byte[headersLength];
                
            for (var i = 0; i < headersBuffer.Length; i += ChunkSize)
            {
                var bytesToWrite = Math.Min(ChunkSize, headersBuffer.Length - i);
                await memoryStream.ReadAsync(headersBuffer, 0, bytesToWrite, cancellationToken);
                
                bytesReads += bytesToWrite;
                OnReadProgressChanged(bytesReads, totalBytes);

            }
                
            var headersString = Encoding.Unicode.GetString(headersBuffer);
                
            var bodyLengthBuffer = new byte[4];
            await memoryStream.ReadAsync(bodyLengthBuffer, 0, bodyLengthBuffer.Length, cancellationToken);
            var bodyLength = BitConverter.ToInt32(bodyLengthBuffer, 0);
                
            bytesReads += bodyLengthBuffer.Length;
            OnReadProgressChanged(bytesReads, totalBytes);
                
            var bodyBuffer = new byte[bodyLength];
                
            for (var i = 0; i < bodyBuffer.Length; i += ChunkSize)
            {
                var bytesToWrite = Math.Min(ChunkSize, bodyBuffer.Length - i);
                await memoryStream.ReadAsync(bodyBuffer, 0, bytesToWrite, cancellationToken);
               
                bytesReads += bytesToWrite;
                OnReadProgressChanged(bytesReads, totalBytes);
            }
                
            var bodyString = Encoding.Unicode.GetString(bodyBuffer);
                
            result.Add("Headers", headersString);
            result.Add("Body", bodyString);
                
            return result;
        } 
    }

    private async Task WriteAsyncInternal(Dictionary<string, string> dictionary, CancellationToken cancellationToken)
    {
        var bytesWrite = 0;

        using (var memoryStream = new MemoryStream())
        {
            var messageType = (MessageType)Enum.Parse(typeof(MessageType), dictionary["MessageType"]);
            var messageTypeByte = BitConverter.GetBytes((int)messageType);
        
            if (messageType == MessageType.HealthCheck)
            {
                var responseLength = messageTypeByte.Length;
                var responseLengthBytes = BitConverter.GetBytes(responseLength);
                
                await memoryStream.WriteAsync(responseLengthBytes, 0, responseLengthBytes.Length, cancellationToken);
                await memoryStream.WriteAsync(messageTypeByte, 0, messageTypeByte.Length, cancellationToken);
            }
            else
            {
                var status = (StatusCode)Enum.Parse(typeof(StatusCode), dictionary["StatusCode"]);
                var statusByte = BitConverter.GetBytes((int)status);

                var headerString = dictionary["Headers"];
                var body = dictionary["Body"];
            
                var headerBytes = Encoding.Unicode.GetBytes(headerString);
                var headerBytesLength = BitConverter.GetBytes(headerBytes.Length);
            
                var bodyBytes = Encoding.Unicode.GetBytes(body);
                var bodyBytesLength = BitConverter.GetBytes(bodyBytes.Length);
            
                var responseLength = messageTypeByte.Length + statusByte.Length + 
                                     headerBytesLength.Length + headerBytes.Length + 
                                     bodyBytesLength.Length + bodyBytes.Length;
                
                var responseLengthBytes = BitConverter.GetBytes(responseLength);
                var totalBytes = responseLengthBytes.Length + responseLength;
                
                await memoryStream.WriteAsync(responseLengthBytes, 0, responseLengthBytes.Length, cancellationToken);
                bytesWrite += responseLengthBytes.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
                
                await memoryStream.WriteAsync(messageTypeByte, 0, messageTypeByte.Length, cancellationToken);
                bytesWrite += messageTypeByte.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
                
                await memoryStream.WriteAsync(statusByte, 0, statusByte.Length, cancellationToken);
                bytesWrite += statusByte.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
            
                await memoryStream.WriteAsync(headerBytesLength, 0, headerBytesLength.Length, cancellationToken);
                bytesWrite += headerBytesLength.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
            
                for (var i = 0; i < headerBytes.Length; i += ChunkSize)
                {
                    var bytesToWrite = Math.Min(ChunkSize, headerBytes.Length - i);
                    await memoryStream.WriteAsync(headerBytes, 0, bytesToWrite, cancellationToken);
                
                    bytesWrite = i + bytesToWrite;
                    OnWriteProgressChanged(bytesWrite, totalBytes);
                }

                await memoryStream.WriteAsync(bodyBytesLength, 0, bodyBytesLength.Length, cancellationToken);
                bytesWrite += bodyBytesLength.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
            
                for (var i = 0; i < bodyBytes.Length; i += ChunkSize)
                {
                    var bytesToWrite = Math.Min(ChunkSize, bodyBytes.Length - i);
                    await memoryStream.WriteAsync(bodyBytes, 0, bytesToWrite, cancellationToken);
                
                    bytesWrite = i + bytesToWrite;
                    OnWriteProgressChanged(bytesWrite, totalBytes);
                }
            }
        
            var buffer = memoryStream.ToArray();
            await _serverPipeStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }
    }
    
    private void OnWriteProgressChanged(int bytesWrite, int totalBytes)
    {
        WriteProgress?.Invoke(this,
            new ProgressEventArgs(bytesWrite, totalBytes, ProgressOperationType.Write));
    }
    
    private void OnReadProgressChanged(int bytesReads, int totalBytes)
    {
        ReadProgress?.Invoke(this, 
            new ProgressEventArgs(bytesReads, totalBytes, ProgressOperationType.Read));
    }

    private static string GetBinaryBitString(byte[] contentBuffer)
    {
        var binaryString = new StringBuilder();
        foreach (byte b in contentBuffer)
        {
            binaryString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            binaryString.Append(" ");
        }

        return binaryString.ToString();
    }

    private void SetGeneralHeaders(Response response)
    {
        response.Headers.Add(Constants.ProtocolHeaderName, Constants.ProtocolName);
        response.Headers.Add(Constants.ProtocolFullNameHeaderName, Constants.ProtocolFullName);
        response.Headers.Add(Constants.ProtocolVersionHeaderName, Constants.ProtocolVersion);
        response.Headers.Add(Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
    }
    
    private void DisposeStreams()
    {
        _serverPipeStream?.Close();
        _serverPipeStream?.Dispose();
        _serverPipeStream = null;
    }
    
    /// <summary>
    ///  Event handler for progress of read operations
    /// </summary>
    public event EventHandler<ProgressEventArgs> ReadProgress;
    
    /// <summary>
    ///  Event handler for progress of write operations 
    /// </summary>
    public event EventHandler<ProgressEventArgs> WriteProgress;
    
    private void ReleaseUnmanagedResources()
    {
        _workers.Values.ToList().ForEach(x => x.StopAsync(CancellationToken.None));
        DisposeStreams();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Server()
    {
        ReleaseUnmanagedResources();
    }

    public IServiceProvider Services { get; }
}