using System.Text;
using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Events;
using Lib.Iteraction.Request;
using Lib.Iteraction.Response;

namespace Lib.Iteraction.Serializator;

public class Serializer : ISerializer
{
    private readonly Encoding _encoding;

    public Serializer(Encoding encoding)
    {
        _encoding = encoding;
    }

    public Task<MemoryStream> SerializeRequest(RequestBase request)
    {
         var requestMemoryStream = new MemoryStream();
         var binaryWriter = new BinaryWriter(requestMemoryStream, _encoding);
         
         var headers = request.Headers;
         var headersJsonString = JsonSerializer.Serialize(headers);
         var headersJsonStringBytes = _encoding.GetBytes(headersJsonString);
         var headersJsonStringLength = headersJsonStringBytes.Length;
         var headersJsonStringLengthBytes = BitConverter.GetBytes(headersJsonStringLength);
         
         var bodyJsonString = request.Body;
         var bodyJsonStringBytes = _encoding.GetBytes(bodyJsonString);
         var bodyJsonStringLength = bodyJsonStringBytes.Length;
         var bodyJsonStringLengthBytes = BitConverter.GetBytes(bodyJsonStringLength);
         
         var contentByteLength = bodyJsonStringLength + headersJsonStringLengthBytes.Length + 
                                 headersJsonStringLength + bodyJsonStringLengthBytes.Length;
         
         binaryWriter.Write(contentByteLength);
         
         binaryWriter.Write( headersJsonStringLength );
         binaryWriter.Write( headersJsonStringBytes );
         binaryWriter.Write( bodyJsonStringLength );
         binaryWriter.Write( bodyJsonStringBytes );
         
         return Task.FromResult(requestMemoryStream);
    }
    
    public Task<RequestBase> DeserializeRequest(MemoryStream request)
    {
        var streamWriter = new BinaryReader(request, _encoding);
        
        var headersByteLength = streamWriter.ReadInt32();
        var headersStringJsonBytes = streamWriter.ReadBytes(headersByteLength);
        var headersJsonString = _encoding.GetString(headersStringJsonBytes); 
        
        var bodyByteLength = streamWriter.ReadInt32();
        var bodyStringJsonBytes = streamWriter.ReadBytes(bodyByteLength);
        var bodyStringJson = _encoding.GetString(bodyStringJsonBytes);
        
        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJsonString);
        
        var requestBase = new Request.Request
        {
            Headers = headers,
            Body =  bodyStringJson
        };
        
        return Task.FromResult<RequestBase>(requestBase);
    }

    public Task<MemoryStream> SerializeResponse(ResponseBase response)
    {
        var requestMemoryStream = new MemoryStream();
        var binaryWriter = new BinaryWriter(requestMemoryStream, _encoding);
        
        var statusCode = (int)response.StatusCode;
        var statusCodeBytes = BitConverter.GetBytes(statusCode);

        var headers = response.Headers;
        var headersJsonString = JsonSerializer.Serialize(headers);
        var headersJsonStringBytes = _encoding.GetBytes(headersJsonString);
        var headersJsonStringLength = headersJsonStringBytes.Length;
        var headersJsonStringLengthBytes = BitConverter.GetBytes(headersJsonStringLength);
         
        var bodyJsonString = response.Body; 
        var bodyJsonStringBytes = _encoding.GetBytes(bodyJsonString);
        var bodyJsonStringLength = bodyJsonStringBytes.Length;
        var bodyJsonStringLengthBytes = BitConverter.GetBytes(bodyJsonStringLength);
         
        var contentByteLength = statusCodeBytes.Length + 
                                headersJsonStringLength + headersJsonStringLengthBytes.Length + 
                                bodyJsonStringLength + bodyJsonStringLengthBytes.Length;
        
        binaryWriter.Write(contentByteLength);
        
        binaryWriter.Write(statusCode);
        
        binaryWriter.Write(headersJsonStringLength);
        binaryWriter.Write(headersJsonStringBytes);
        
        binaryWriter.Write(bodyJsonStringLength);
        binaryWriter.Write(bodyJsonStringBytes);
        
        return Task.FromResult(requestMemoryStream);
    }

    public Task<ResponseBase> DeserializeResponse(MemoryStream response)
    {
        var streamWriter = new BinaryReader(response, _encoding);
        
        var statusCode = (StatusCode)streamWriter.ReadInt32();
        
        var headersByteLength = streamWriter.ReadInt32();
        var headersStringJsonBytes = streamWriter.ReadBytes(headersByteLength);
        var headersJsonString = _encoding.GetString(headersStringJsonBytes); 
        
        var bodyByteLength = streamWriter.ReadInt32();
        var bodyStringJsonBytes = streamWriter.ReadBytes(bodyByteLength);
        var bodyStringJson = _encoding.GetString(bodyStringJsonBytes);
        
        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJsonString);
        
        var requestBase = new Response.Response(statusCode)
        {
            Headers = headers,
            Body =  bodyStringJson
        };
        
        return Task.FromResult<ResponseBase>(requestBase);
    }

    private void OnProgressChange(
        int bytesWriteCount, 
        int totalBytesWriteCount, 
        ProgressOperationType operationType
        )
    {
        ByteProgress?.Invoke(this,
            new ProgressEventArgs(bytesWriteCount, totalBytesWriteCount, operationType));
    }

    public event EventHandler<ProgressEventArgs>? ByteProgress;
}