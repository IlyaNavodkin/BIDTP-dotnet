using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Events;
using BIDTP.Dotnet.Core.Iteraction.Interfaces;
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Core.Iteraction.Providers;
using BIDTP.Dotnet.Core.Iteraction.Utills;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BIDTP.Dotnet.Core.Iteraction;

/// <summary>
///  Class of the server. 
/// </summary>
public class Server : IHost
{
    private readonly Dictionary<string, Func<Context, Task>[]> _routeHandlers;
    private readonly ConcurrentDictionary<string, IHostedService> _workers;
    private readonly SemaphoreSlim _streamSemaphore;
    private NamedPipeServerStream _serverPipeStream;
    
    /// <summary>
    ///  Reconnect time rate of the server
    /// </summary>
    public int ReconnectTimeRate { get; }
    
    /// <summary>
    ///  The name of the server for connection
    /// </summary>
    public string ServerName { get; }
    
    /// <summary>
    ///  Json serializer options for request\response serialization
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; }
    
    /// <summary>
    ///  The chunk size for the transmission data
    /// </summary>
    private int ChunkSize { get; set; }

    /// <summary>
    ///  Server of the BIDTP protocol
    /// </summary>
    /// <param name="options"> The options of the server </param>
    /// <param name="routeHandlers"> The route handlers </param>
    /// <param name="serviceProvider"> The service provider </param>
    public Server(ServerOptions options, Dictionary<string, 
        Func<Context, Task>[]> routeHandlers,  IServiceProvider serviceProvider)
    {
        ServerName = options.ServerName;
        Services = serviceProvider;
        ChunkSize = options.ChunkSize;
        ReconnectTimeRate = options.ReconnectTimeRate;
        JsonSerializerOptions = options.JsonSerializerOptions;

        _routeHandlers = routeHandlers;
        
        _workers = new ConcurrentDictionary<string, IHostedService>(); 
        
        _streamSemaphore  = new SemaphoreSlim(1, 1);
    }
    
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
                
                _serverPipeStream = new NamedPipeServerStream(ServerName, PipeDirection.InOut, 
                    NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message);
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
            
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersString);
            
            var request = new Request
            {
                Headers = headers
            };
            
            request.SetBody(body);
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
        
        var errorResponse = new Response(StatusCode.ServerError);
        errorResponse.SetBody(error);
        
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
        
        var errorBody = JsonSerializer.Serialize(error, JsonSerializerOptions);

        var notExistResponse = new Response(StatusCode.NotFound);
        notExistResponse.SetBody(errorBody);
                
        SetGeneralHeaders(notExistResponse);

        return ConvertToDictionary(notExistResponse);
    }

    private Dictionary<string, string> ConvertToDictionary(Response response)
    {
        var dictionary = new Dictionary<string, string>();
                
        // var headerString = JsonConvert.SerializeObject(response.Headers);
        var headerString = JsonSerializer.Serialize(response.Headers,JsonSerializerOptions);
        
        var bodyString = response.GetBody<string>();
                
        dictionary.Add(nameof(MessageType), response.MessageType.ToString());
        dictionary.Add(nameof(StatusCode), response.StatusCode.ToString());
        dictionary.Add("Headers", headerString);
        dictionary.Add("Body", bodyString);
                
        return dictionary;
    }
    
    public Encoding Encoding { get; set; } = Encoding.Unicode;
    
    private async Task<Dictionary<string,string>> ReadAsyncInternal(CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();
        
        var bytesRead = 0;
        
        var messageLengthBytes = new byte[4];
        var messageTypeByteReadCount = await _serverPipeStream
            .ReadAsync(messageLengthBytes, 
                0, 
                messageLengthBytes.Length,
                cancellationToken
                );
        
        var messageLength = BitConverter.ToInt32(messageLengthBytes, 0);
        
        var contentBuffer = new byte[messageLength];
        var messageLengthByteReadCount = await _serverPipeStream
            .ReadAsync(
                contentBuffer, 
                0, 
                contentBuffer.Length, 
                cancellationToken
                );
        
        using (var memoryStream = new MemoryStream(contentBuffer))
        {
            var binaryReader = new BinaryReader(memoryStream, Encoding);

            var messageType = (MessageType)binaryReader.ReadInt32();
            result.Add(nameof(MessageType), messageType.ToString());
            
            if(messageType == MessageType.HealthCheck) return result;
            
            var headerString = BytesConvertUtills.ReadStringBytes(
                cancellationToken,
                binaryReader,
                bytesRead,
                messageLengthByteReadCount,
                Encoding, 
                ChunkSize,
                OnReadProgressChanged
            );
            
            result.Add("Headers",headerString);
            
            var bodyString = BytesConvertUtills.ReadStringBytes(
                cancellationToken,
                binaryReader,
                bytesRead,
                messageLengthByteReadCount,
                Encoding, 
                ChunkSize,
                OnReadProgressChanged
            );
            
            result.Add("Body",bodyString);
                
            ReadCompleted?.Invoke( this, EventArgs.Empty);
            
            return result;
        } 
    }

    private async Task WriteAsyncInternal(Dictionary<string, string> dictionary, CancellationToken cancellationToken)
    {
        using (var memoryStream = new MemoryStream())
        {
            var binaryWriter = new BinaryWriter(memoryStream, Encoding);
            
            var messageType = (MessageType)Enum
                .Parse(typeof(MessageType), dictionary[nameof(MessageType)]);
            
            binaryWriter.Write((int)messageType);
            
            if (messageType == MessageType.General)
            {
                var statusCode = (StatusCode)Enum.Parse(typeof(StatusCode), dictionary[nameof(StatusCode)]);
                binaryWriter.Write((int) statusCode);
                
                var bytesWriteCount = 0;
                
                var headerString = dictionary["Headers"];
                var headerBuffer = Encoding.GetBytes(headerString);
                
                var bodyString = dictionary["Body"];
                var bodyBuffer = Encoding.GetBytes(bodyString);

                var totalBytesWriteCount =
                    headerBuffer.Length + bodyBuffer.Length;
                
                BytesConvertUtills.WriteStringBytes(
                    cancellationToken, 
                    binaryWriter, 
                    headerBuffer, 
                    ref bytesWriteCount, 
                    totalBytesWriteCount, 
                    ChunkSize,
                    OnWriteProgressChanged
                );
                
                BytesConvertUtills.WriteStringBytes(
                    cancellationToken, 
                    binaryWriter, 
                    bodyBuffer, 
                    ref bytesWriteCount, 
                    totalBytesWriteCount, 
                    ChunkSize,
                    OnWriteProgressChanged
                );
            }

            var buffer = memoryStream.ToArray();
            var allLength = buffer.Length;
            var allLengthBuffer = BitConverter.GetBytes(allLength);
            
            buffer = allLengthBuffer.Concat(buffer).ToArray();
            
            WriteCompleted?.Invoke( this, EventArgs.Empty);
            
            await _serverPipeStream
                .WriteAsync(
                    buffer, 
                    0, 
                    buffer.Length, 
                    cancellationToken
                    );
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
    ///  Occurs when write progress
    /// </summary>
    public event EventHandler<EventArgs> WriteCompleted;    
    /// <summary>
    ///  Occurs when read progress
    /// </summary>
    public event EventHandler<EventArgs> ReadCompleted;
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
