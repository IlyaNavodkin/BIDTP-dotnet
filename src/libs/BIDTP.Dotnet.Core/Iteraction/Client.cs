using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BIDTP.Dotnet.Iteraction.Dtos;
using BIDTP.Dotnet.Iteraction.Enums;
using BIDTP.Dotnet.Iteraction.Events;
using BIDTP.Dotnet.Iteraction.Options;
using Newtonsoft.Json;

namespace BIDTP.Dotnet.Iteraction;
/// <summary>
///  Client 
/// </summary>
public class Client
{
    private readonly SemaphoreSlim _pipeSemaphore;
    private NamedPipeClientStream _clientPipeStream;
    private CancellationTokenSource _cancellationTokenSource;
    
    /// <summary>
    ///  Connection is starting
    /// </summary>
    public bool IsConnectionStarting;
    /// <summary>
    ///  The name of the pipe 
    /// </summary>
    public string PipeName { get; }
    /// <summary>
    ///  The chunk size for the transmission data
    /// </summary>
    public int ChunkSize { get; set; }
    /// <summary>
    ///  The time rate of the life check 
    /// </summary>
    public int LifeCheckTimeRate { get; }
    /// <summary>
    ///  The time rate of the reconnect 
    /// </summary>
    public int ReconnectTimeRate { get; }
    /// <summary>
    ///  The timeout of the connect 
    /// </summary>
    public int ConnectTimeout { get; }
    
    /// <summary>
    ///  The status of the connection 
    /// </summary>
    public bool IsHealthCheckConnected;
    
    /// <summary>
    ///  Create a new SIDTPClient 
    /// </summary>
    /// <param name="options"> The options of the client </param>
    public Client(ClientOptions options)
    {
        _pipeSemaphore = new SemaphoreSlim(1, 1);
        
        PipeName = options.PipeName;
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
                
            _clientPipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);

            await _clientPipeStream.ConnectAsync(ConnectTimeout, _cancellationTokenSource.Token);

            await CheckConnection();
            
            _ = CheckConnectionBackground();
        }
        catch (Exception)
        {
            DisposeStream();
        }
        
        IsConnectionStarting = false;
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
                Debug.WriteLine($"[LifeCheck Error]: {operationCanceledException.Message}");
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

            Debug.WriteLine($"[LifeCheck Request]: ping");
                
            var response = await ReadAsyncInternal(_cancellationTokenSource.Token);
                
            IsHealthCheckConnected = !string.IsNullOrEmpty(response[nameof(MessageType)]);
            IsLifeCheckConnectedChanged ?.Invoke(this, IsHealthCheckConnected);
                
            Debug.WriteLine($"[LifeCheck Response]: pong");
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"[LifeCheck Error]: {exception.Message}");
            DisposeStream();
        }
        finally
        {
            _pipeSemaphore.Release();
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
            
            var headersJsonString = JsonConvert.SerializeObject(request.Headers);
            dictionary.Add("Headers", headersJsonString);
            
            dictionary.Add("Body", request.Body);
            
            await WriteAsyncInternal(dictionary, cancellationToken);
            
            var result = await ReadAsyncInternal(cancellationToken);
            
            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(result["Headers"]);
            var statusCode = (StatusCode) Enum.Parse(typeof(StatusCode), result[nameof(StatusCode)]);
            var response = new Response(statusCode)
            {
                Body = result["Body"],
                Headers = headers
            };
            
            return response;

        }
        catch (Exception exception)
        {
            Debug.WriteLine($"[Client Error]: {exception.Message}");
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
        
        var bytesReads = 0;
        
        var messageLengthBuffer = new byte[4];
        await _clientPipeStream.ReadAsync(messageLengthBuffer, bytesReads, messageLengthBuffer.Length);
        var messageLength = BitConverter.ToInt32(messageLengthBuffer, 0);
        
        bytesReads += messageLengthBuffer.Length;

        var contentBuffer = new byte[messageLength];
        await _clientPipeStream.ReadAsync(contentBuffer, 0, contentBuffer.Length, cancellationToken);
        
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
            
            var statusCodeLengthBuffer = new byte[4];
            await memoryStream.ReadAsync(statusCodeLengthBuffer, 0, statusCodeLengthBuffer.Length, cancellationToken);
            var statusCode = (StatusCode) BitConverter.ToInt32(statusCodeLengthBuffer, 0);
                
            bytesReads += statusCodeLengthBuffer.Length;
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
                
            result.Add(nameof(StatusCode), statusCode.ToString());
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
                var headerString = dictionary["Headers"];
                var body = dictionary["Body"];
            
                var headerBytes = Encoding.Unicode.GetBytes(headerString);
                var headerBytesLength = BitConverter.GetBytes(headerBytes.Length);
            
                var bodyBytes = Encoding.Unicode.GetBytes(body);
                var bodyBytesLength = BitConverter.GetBytes(bodyBytes.Length);
            
                var responseLength = messageTypeByte.Length + headerBytesLength.Length + headerBytes.Length + bodyBytesLength.Length + bodyBytes.Length;
                var responseLengthBytes = BitConverter.GetBytes(responseLength);
                var totalBytes = responseLengthBytes.Length + responseLength;
                
                await memoryStream.WriteAsync(responseLengthBytes, 0, responseLengthBytes.Length, cancellationToken);
                bytesWrite += responseLengthBytes.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);

                await memoryStream.WriteAsync(messageTypeByte, 0, messageTypeByte.Length, cancellationToken);
                bytesWrite += messageTypeByte.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
                
                await memoryStream.WriteAsync(headerBytesLength, 0, headerBytesLength.Length, cancellationToken);
                bytesWrite += headerBytesLength.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
            
                for (var i = 0; i < headerBytes.Length; i += ChunkSize)
                {
                    var bytesToWrite = Math.Min(ChunkSize, headerBytes.Length - i);
                    await memoryStream.WriteAsync(headerBytes, 0, bytesToWrite, cancellationToken);
                
                    bytesWrite += bytesToWrite;
                    OnWriteProgressChanged(bytesWrite, totalBytes);
                }
                
                await memoryStream.WriteAsync(bodyBytesLength, 0, bodyBytesLength.Length, cancellationToken);
                bytesWrite += bodyBytesLength.Length;
                OnWriteProgressChanged(bytesWrite, totalBytes);
                
                for (var i = 0; i < bodyBytes.Length; i += ChunkSize)
                {
                    var bytesToWrite = Math.Min(ChunkSize, bodyBytes.Length - i);
                    await memoryStream.WriteAsync(bodyBytes, 0, bytesToWrite, cancellationToken);
                
                    bytesWrite += bytesToWrite;
                    OnWriteProgressChanged(bytesWrite, totalBytes);
                }
            }
        
            var buffer = memoryStream.ToArray();
            await _clientPipeStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
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
        Debug.WriteLine($"[Dispose stream]: Start");
        
        IsHealthCheckConnected = false;
        IsLifeCheckConnectedChanged ?.Invoke(this, IsHealthCheckConnected);
        
        _cancellationTokenSource?.Cancel();
        
        _clientPipeStream?.Close();
        _clientPipeStream?.Dispose();
        _clientPipeStream = null;
        
        Debug.WriteLine($"[Dispose stream]: Success");
    }
    
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