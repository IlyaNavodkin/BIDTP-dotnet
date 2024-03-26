using System;
using System.Collections.Generic;
using System.Text.Json;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Helpers;

namespace BIDTP.Dotnet.Core.Iteraction.Dtos;

/// <summary>
///  The response from the server
///  
/// Schema sample:
/// 
///  Field Name         Type                Size (bytes)
/// ----------------------------------------------------
///  MessageLength     int                  4 
///  MessageType       int (enum)           4
///  StatusCode        int (enum)           4
///  HeadersLength     int                  4      
///  HeadersContent    string (unicode)     var      
///  BodyLength        int                  4             
///  BodyContent       string (unicode)     var   
/// 
/// </summary>
public class Response 
{
    private string _body = string.Empty;
    
    /// <summary>
    ///  The type of the message, general - is default message logic with Response and Request
    /// </summary>
    public readonly MessageType MessageType = MessageType.General;
    
    /// <summary>
    /// Response status code
    /// </summary>
    public StatusCode StatusCode { get; }
    
    /// <summary>
    ///  The headers of the request
    /// </summary>
    public Dictionary<string, string>  Headers { get; set; }

    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="type"> The status code of the response </param>
    public Response(StatusCode type)
    {
        StatusCode = type;
        
        Headers = new Dictionary<string, string>();
    }
    
    /// <summary>
    ///  Get the deserialized body as a T
    /// </summary>
    /// <typeparam name="T"> The type of the body </typeparam>
    /// <param name="options"> The json serializer options </param>
    /// <returns> The body of the response </returns>
    public T GetBody<T>(JsonSerializerOptions options = default)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)_body;
        }

        return JsonSerializer.Deserialize<T>(_body, JsonHelper.GetDefaultJsonSerializerOptions());
    }

    /// <summary>
    ///  Set the serialized body as a T
    /// </summary>
    /// <typeparam name="T"> The type of the body </typeparam>
    /// <param name="obj"> The body of the request </param>
    /// <param name="options"> The json serializer options </param>
    public void SetBody<T>(T obj, JsonSerializerOptions options = default) 
    {
        if (typeof(T) == typeof(string))
        {
            _body = obj as string;
        }
        else
        {
            _body = JsonSerializer.Serialize(obj, JsonHelper.GetDefaultJsonSerializerOptions());
        }
    }
    
    /// <summary>
    ///  Validate the response
    /// </summary>
    /// <exception cref="Exception"> Thrown if the response is invalid </exception>
    public void Validate()
    {
        if (_body is null) throw new Exception("Response body can't be null, check the response");
        if (Headers is null) throw new Exception("Response headers can't be null, check the response");
    }
}