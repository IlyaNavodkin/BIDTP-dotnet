using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Events;
using BIDTP.Dotnet.Core.Iteraction.Interfaces;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace BIDTP.Dotnet.Core.Iteraction;

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
    private int ReconnectTimeRate { get; }
    
    /// <summary>
    ///  The chunk size for the transmission data
    /// </summary>
    public int ChunkSize { get; set; }
    
    /// <summary>
    ///  Server of the BIDTP protocol
    /// </summary>
    /// <param name="pipeName"> The name of the pipe </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    /// <param name="routeHandlers"> The route handlers </param>
    /// <param name="serviceProvider"> The service provider </param>
    public Server(string pipeName, int chunkSize, int reconnectTimeRate,
        Dictionary<string, Func<Context, Task>[]> routeHandlers, IServiceProvider serviceProvider)
    {
        _pipeName = pipeName;
        ChunkSize = chunkSize;
        ReconnectTimeRate = reconnectTimeRate;

        _routeHandlers = routeHandlers;
        Services = serviceProvider;
        
        _workers = new ConcurrentDictionary<string, IHostedService>(); 
        
        _streamSemaphore  = new SemaphoreSlim(1, 1);
    }
    
    /// <summary>
    ///  Server of the BIDTP protocol
    /// </summary>
    /// <param name="pipeName"> The name of the pipe </param>
    /// <param name="chunkSize"> The chunk size for the transmission data </param>
    /// <param name="reconnectTimeRate"> The time rate of the reconnect </param>
    /// <param name="routeHandlers"> The route handlers </param>
    public Server(string pipeName, int chunkSize, int reconnectTimeRate,
        Dictionary<string, Func<Context, Task>[]> routeHandlers)
    {
        _pipeName = pipeName;
        ChunkSize = chunkSize;
        ReconnectTimeRate = reconnectTimeRate;

        _routeHandlers = routeHandlers;
        
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
        Console.WriteLine("[SERVER: StartAsync]: Trying to start server");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if(_serverPipeStream is not null) throw new Exception("Stream already created");
                
                _serverPipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 
                    1, PipeTransmissionMode.Message);
                await _serverPipeStream.WaitForConnectionAsync(cancellationToken);
                
                Console.WriteLine("[SERVER: StartAsync]: Server wait for connection");
                
                await Listen(cancellationToken);
                
                Console.WriteLine("[SERVER: StartAsync]: Server listening");
            }
            catch (Exception)
            {
                DisposeStreams();
            }

            try
            {
                await Task.Delay(ReconnectTimeRate, cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                Console.WriteLine(exception);
            }
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
                
                dictionary.TryGetValue(nameof(MessageType), out var messageTypeString);
                
                if(messageTypeString is null) throw new Exception("Message type not found");
                
                var messageType = (MessageType) Enum.Parse(typeof(MessageType), messageTypeString);

                if (messageType is MessageType.HealthCheck)
                {
                    result.Add(nameof(MessageType), MessageType.HealthCheck.ToString());
                }
                else
                {
                    result = await ServeRequest(dictionary);
                }

                await WriteAsyncInternal(result, cancellationToken);
            }
            finally
            {
                _streamSemaphore.Release();
            }
        }
    }
    
    private async Task<Dictionary<string,string>> ServeRequest(Dictionary<string,string> requestDictionary)
    {
        try
        {
            var body = requestDictionary["Body"];
            
            var headersString = requestDictionary["Headers"];
            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString);
            
            var request = new Request
            {
                Body = body,
                Headers = headers
            };
            
            request.Headers.TryGetValue(Constants.Constants.RouteHeaderName, out var route);
            
            if(route is null) throw new Exception("Route key is not found. Add route header in the request!"); 
            
            var serverRouteNotExist = !_routeHandlers.TryGetValue(route, out var handlers);
        
            if (serverRouteNotExist)
            {
                return HandleRouteNotExistRequest(route);
            }
        
            return await HandleGeneralResponse(request, handlers);
        }
        catch (Exception exception)
        {
            return HandleServerInternalErrorResponse(exception);
        }
    }

    private async Task<Dictionary<string, string>> HandleGeneralResponse(Request request, Func<Context, Task>[] handlers)
    {
        var context = new Context(request, Services);
        
        foreach (var handler in handlers)
        {
            var methodInfo = handler.Method;
            var attributes = methodInfo
                .GetCustomAttributes(true)
                .Where(atr => atr is IMethodScopedPreInvokable)
                .Cast<IMethodScopedPreInvokable>()
                .ToArray(); 
            
            foreach (var attribute in attributes)
            {
                await attribute.Invoke(context);
                if (context.Response != null) break;
            }
            
            if (context.Response == null)
            {
                await handler(context);
                if (context.Response != null) break;
            }
        }
        
        if (context.Response is null) throw new InvalidOperationException("Server response is not set!");
                
        var response = context.Response;
        SetGeneralHeaders(response);
        
        response.Validate();
                
        var result = ConvertToDictionary(response);
                
        return result;
    }

    private Dictionary<string, string> HandleServerInternalErrorResponse(Exception exception)
    {
        Console.WriteLine(exception);
        
        var error = new Error 
        {
            Message = "Internal server error!",
            Description = exception.Message,
            ErrorCode = (int) InternalServerErrorType.DispatcherExceptionError,
            StackTrace = exception.StackTrace
        };
        
        var errorResponse = new Response(StatusCode.ServerError)
        {
            Body = JsonConvert.SerializeObject(error)
        };
            
        SetGeneralHeaders(errorResponse);
                
        var result = ConvertToDictionary(errorResponse);
                
        return result;
    }

    private Dictionary<string, string> HandleRouteNotExistRequest(string route)
    {
        var error = new Error
        {
            Message = $"Route {route} is not found!",
            ErrorCode = (int) InternalServerErrorType.RouteNotFoundError
        };
        
        var notExistResponse = new Response(StatusCode.NotFound)
        {
            Body = JsonConvert.SerializeObject(error)
        };
                
        SetGeneralHeaders(notExistResponse);

        return ConvertToDictionary(notExistResponse);
    }

    private static Dictionary<string, string> ConvertToDictionary(Response response)
    {
        var dictionary = new Dictionary<string, string>();
                
        var headerString = JsonConvert.SerializeObject(response.Headers);
        var bodyString = response.Body;
                
        dictionary.Add(nameof(MessageType), response.MessageType.ToString());
        dictionary.Add(nameof(StatusCode), response.StatusCode.ToString());
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

            result.Add(nameof(MessageType), messageTypeValue.ToString());
            
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
            var messageType = (MessageType)Enum.Parse(typeof(MessageType), dictionary[nameof(MessageType)]);
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
                var status = (StatusCode)Enum.Parse(typeof(StatusCode), dictionary[nameof(StatusCode)]);
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
    
    private void SetGeneralHeaders(Response response)
    {
        response.Headers.Add(Constants.Constants.ProtocolHeaderName, Constants.Constants.ProtocolName);
        response.Headers.Add(Constants.Constants.ProtocolFullNameHeaderName, Constants.Constants.ProtocolFullName);
        response.Headers.Add(Constants.Constants.ProtocolVersionHeaderName, Constants.Constants.ProtocolVersion);
        response.Headers.Add(Constants.Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
    }
    
    private void DisposeStreams()
    {
        _serverPipeStream?.Close();
        _serverPipeStream?.Dispose();
        _serverPipeStream = null;
        
        Console.WriteLine("[SERVER: Dispose stream]: Dispose stream");
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

    /// <summary>
    ///  Event handler for progress of read operations
    /// </summary>
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    ~Server()
    {
        ReleaseUnmanagedResources();
    }

    /// <inheritdoc />
    public IServiceProvider Services { get; }
}
