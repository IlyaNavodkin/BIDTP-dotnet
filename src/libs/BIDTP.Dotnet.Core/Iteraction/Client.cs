using System;
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
using BIDTP.Dotnet.Core.Iteraction.Options;
using BIDTP.Dotnet.Core.Iteraction.Utills;

namespace BIDTP.Dotnet.Core.Iteraction;
/// <summary>
///  Client 
/// </summary>
public class Client
{
    private readonly SemaphoreSlim _pipeSemaphore;
    private NamedPipeClientStream _clientPipeStream;
    private CancellationTokenSource _cancellationTokenSource;
    
    /// <summary>
    ///  The name of the server for connection
    /// </summary>
    private string ServerName { get; }
    
    /// <summary>
    ///  Json serializer options
    /// </summary>
    private JsonSerializerOptions JsonSerializerOptions { get; }
    
    /// <summary>
    ///  Connection is starting
    /// </summary>
    public bool IsConnectionStarting;
    
    /// <summary>
    ///  The chunk size for the transmission data
    /// </summary>
    private int ChunkSize { get; set; }
    
    /// <summary>
    ///  The time rate of the life check 
    /// </summary>
    private int LifeCheckTimeRate { get; }
    
    /// <summary>
    ///  The time rate of the reconnect 
    /// </summary>
    public int ReconnectTimeRate { get; }
    
    /// <summary>
    ///  The timeout of the connect 
    /// </summary>
    private int ConnectTimeout { get; }
    
    /// <summary>
    ///  The status of the connection 
    /// </summary>
    public bool IsHealthCheckConnected = true;
    
    /// <summary>
    ///  Create a new SIDTPClient 
    /// </summary>
    /// <param name="options"> The options of the client </param>
    public Client(ClientOptions options)
    {
        _pipeSemaphore = new SemaphoreSlim(1, 1);
        
        JsonSerializerOptions = options.JsonSerializerOptions;
        ServerName = options.ServerName;
        ChunkSize = options.ChunkSize;
        LifeCheckTimeRate = options.LifeCheckTimeRate;
        ReconnectTimeRate = options.ReconnectTimeRate;
        ConnectTimeout = options.ConnectTimeout;
    }


    /// <summary>
    ///  Connect to the server and check the health time to connect
    /// </summary>
    /// <param name="cancellationTokenSource"> The cancellation token source. Stopped client connection  </param>
    /// <exception cref="Exception"> If the connection is starting  </exception>
    public async Task ConnectToServer( CancellationTokenSource  cancellationTokenSource)
    {
        if (IsConnectionStarting) throw new Exception("Connection is starting");
        
        IsConnectionStarting = true;
        _cancellationTokenSource = cancellationTokenSource;
        
        try
        {
            if(_clientPipeStream is not null) throw new Exception("Stream already created");
                
            _clientPipeStream = new NamedPipeClientStream(".", ServerName, PipeDirection.InOut);

            await _clientPipeStream.ConnectAsync(ConnectTimeout, _cancellationTokenSource.Token);

            await CheckConnection();
            
            // _ = CheckConnectionBackground();
        }
        catch (Exception)
        {
            DisposeStream();
        }
        
        // IsConnectionStarting = false;
    }
    
    private async Task CheckConnectionBackground()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            await CheckConnection();
            
            try
            {
                await Task.Delay(LifeCheckTimeRate, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Console.WriteLine($"[CLIENT: LifeCheck Error]: {operationCanceledException.Message}");
            }
        }
        
        DisposeStream();
    }
    
    private async Task CheckConnection()
    {
        try
        {
            await _pipeSemaphore.WaitAsync(_cancellationTokenSource.Token);

            if (!_clientPipeStream.IsConnected) throw new Exception("PipeClient is not connected");

            var dictionary = new Dictionary<string, string>();

            dictionary.Add(nameof(MessageType), MessageType.HealthCheck.ToString());

            await WriteAsyncInternal(dictionary, _cancellationTokenSource.Token);
            
            var response = await ReadAsyncInternal(_cancellationTokenSource.Token);
                
            IsHealthCheckConnected = !string.IsNullOrEmpty(response[nameof(MessageType)]);
            
        }
        catch (Exception exception)
        {
            Console.WriteLine($"[CLIENT: LifeCheck Error]: {exception.Message}");
            DisposeStream();
        }
        finally
        {
            _pipeSemaphore.Release();
            
            IsLifeCheckConnectedChanged ?.Invoke(this, IsHealthCheckConnected);
        }
    }

    /// <summary>
    ///  Writes a request to the server and waits for a response 
    /// </summary>
    /// <param name="request"> Request object </param>
    /// <param name="cancellationToken"> Cancellation token </param>
    /// <returns> Response object </returns>
    /// <exception cref="Exception"> Thrown if the pipe is not connected or if the request is invalid </exception>
    public async Task<Response> WriteRequestAsync(Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _pipeSemaphore.WaitAsync(cancellationToken);

            if(_clientPipeStream is null) throw new Exception("PipeClient is null, connect first");
            if(!IsHealthCheckConnected) throw new Exception("PipeClient is not connected, wait for connection");
            if(!_clientPipeStream.IsConnected) throw new Exception("PipeClient is not connected");
            
            SetGeneralHeaders(request);
            request.Validate();
            
            var dictionary = new Dictionary<string, string>();

            dictionary.Add(nameof(MessageType), request.MessageType.ToString());
            
            var headersJsonString = JsonSerializer.Serialize(request.Headers, JsonSerializerOptions);
            
            dictionary.Add("Headers", headersJsonString);
            
            dictionary.Add("Body", request.GetBody<string>());
            
            await WriteAsyncInternal(dictionary, cancellationToken);
            
            var result = await ReadAsyncInternal(cancellationToken);
            
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(result["Headers"]);
            var statusCode = (StatusCode) Enum.Parse(typeof(StatusCode), result[nameof(StatusCode)]);
            
            var response = new Response(statusCode)
            {
                Headers = headers
            };
            
            response.SetBody(result["Body"]);
            
            return response;

        }
        catch (Exception exception)
        {
            Console.WriteLine($"[CLIENT: Error]: {exception.Message}");
            throw;
        }
        finally
        {
            _pipeSemaphore.Release();
        }
    }
    
    private void SetGeneralHeaders(Request request)
    {
        request.Headers.Add(Constants.Constants.ProtocolHeaderName, Constants.Constants.ProtocolName);
        request.Headers.Add(Constants.Constants.ProtocolFullNameHeaderName, Constants.Constants.ProtocolFullName);
        request.Headers.Add(Constants.Constants.ProtocolVersionHeaderName, Constants.Constants.ProtocolVersion);
        request.Headers.Add(Constants.Constants.ResponseProcessIdHeaderName, Process.GetCurrentProcess().Id.ToString());
    }

    private async Task<Dictionary<string,string>> ReadAsyncInternal(CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();
        
        var bytesRead = 0;
        
        var messageLengthBuffer = new byte[4];
        var messageTypeByteReadCount = await _clientPipeStream.ReadAsync(
            messageLengthBuffer, 
            0, 
            messageLengthBuffer.Length, 
            cancellationToken
        );

        var messageLength = BitConverter.ToInt32(messageLengthBuffer, 0);
        
        var contentBuffer = new byte[messageLength];
        var messageLengthByteReadCount = await _clientPipeStream.ReadAsync(
            contentBuffer, 
            0, 
            contentBuffer.Length, 
            cancellationToken
        );

        using (var memoryStream = new MemoryStream(contentBuffer))
        {
            var binaryReader = new BinaryReader(memoryStream);
            
            var messageTypeBuffer = (MessageType) binaryReader.ReadInt32();
            
            result.Add(nameof(MessageType), messageTypeBuffer.ToString());
        
            if (messageTypeBuffer is MessageType.HealthCheck) return result;
            
            var statusCode = (StatusCode) binaryReader.ReadInt32();
            
            result.Add(nameof(StatusCode), statusCode.ToString());
            
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
            
            ReadCompleted?.Invoke(this, EventArgs.Empty);
            
            return result;
        }
    }
    
    public Encoding Encoding { get; set; } = Encoding.Unicode;
    private async Task WriteAsyncInternal(Dictionary<string, string> dictionary, CancellationToken cancellationToken)
    {
        using (var memoryStream = new MemoryStream())
        {
            var binaryWriter = new BinaryWriter(memoryStream, Encoding);
            
            var messageType = (MessageType)Enum.Parse(typeof(MessageType), dictionary[nameof(MessageType)]);
            binaryWriter.Write((int)messageType);
           
            if (messageType == MessageType.General)
            {
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
            
            await _clientPipeStream.WriteAsync(buffer, 
                0, 
                buffer.Length, 
                cancellationToken
                );
            
            WriteCompleted?.Invoke(this, EventArgs.Empty);
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

    private void DisposeStream()
    {
        IsHealthCheckConnected = false;
        IsLifeCheckConnectedChanged ?.Invoke(this, IsHealthCheckConnected);
        
        _cancellationTokenSource?.Cancel();
        
        _clientPipeStream?.Close();
        _clientPipeStream?.Dispose();
        _clientPipeStream = null;
        
        Console.WriteLine($"[CLIENT: Dispose stream]: Disposed client resources.");
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
    ///  Occurs when read progress
    /// </summary>
    public event EventHandler<ProgressEventArgs> ReadProgress;
    /// <summary>
    ///  Occurs when write progress
    /// </summary>
    public event EventHandler<ProgressEventArgs> WriteProgress;
    /// <summary>
    ///  Is life check connected
    /// </summary>
    public event EventHandler<bool> IsLifeCheckConnectedChanged;
}