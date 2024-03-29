﻿using System;
using System.Collections.Generic;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using Newtonsoft.Json;

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
    /// <summary>
    ///  The type of the message, general - is default message logic with Response and Request
    /// </summary>
    public readonly MessageType MessageType = MessageType.General;
    
    /// <summary>
    /// Response status code
    /// </summary>
    public StatusCode StatusCode { get; }

    /// <summary>
    ///  The body of the request - a Json string
    /// </summary>
    public string Body { get; set; } = string.Empty;

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
    ///  Get the body of the response as a T
    /// </summary>
    /// <typeparam name="T"> The type of the body </typeparam>
    /// <returns> The body of the response </returns>
    public T GetBody<T>() => JsonConvert.DeserializeObject<T>(Body);
    
    /// <summary>
    ///  Validate the response
    /// </summary>
    /// <exception cref="Exception"> Thrown if the response is invalid </exception>
    public void Validate()
    {
        if (Body is null) throw new Exception("Response body can't be null, check the response");
        if (Headers is null) throw new Exception("Response headers can't be null, check the response");
    }
}