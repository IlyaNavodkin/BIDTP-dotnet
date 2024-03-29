﻿using System;
using System.Collections.Generic;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using Newtonsoft.Json;

namespace BIDTP.Dotnet.Core.Iteraction.Dtos;

/// <summary>
///  The request from the client
///
/// Schema sample:
/// 
///  Field Name         Type                Size (bytes)
/// ----------------------------------------------------
///  MessageLength     int                  4 
///  MessageType       int (enum)           4
///  HeadersLength     int                  4      
///  HeadersContent    string (unicode)     var      
///  BodyLength        int                  4             
///  BodyContent       string (unicode)     var   
/// 
/// </summary>
public class Request
{
    /// <summary>
    ///  The type of the message, general - is default message logic with Response and Request
    /// </summary>
    public readonly MessageType MessageType = MessageType.General;
    
    /// <summary>
    ///  The body of the request - a Json string
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    ///  The headers of the request
    /// </summary>
    public Dictionary<string, string>  Headers { get;set;}

    /// <summary>
    ///  Constructor
    /// </summary>
    public Request()
    {
        Headers = new Dictionary<string, string>();
    }

    /// <summary>
    ///  Get the body of the response as a T
    /// </summary>
    /// <typeparam name="T"> The type of the body </typeparam>
    /// <returns> The body of the response </returns>
    public T GetBody<T>() => JsonConvert.DeserializeObject<T>(Body);
    
    /// <summary>
    ///  Set the route of the request
    /// </summary>
    /// <param name="route"> The route of the request </param>
    public void SetRoute(string route)
    {
        Headers[Constants.Constants.RouteHeaderName] = route;
    }

    /// <summary>
    ///  Validate the request 
    /// </summary>
    /// <exception cref="Exception"> Thrown if the request is invalid </exception>
    public void Validate()
    {
        if (Body is null) throw new Exception("Request body can't be null, check the request");
        if (Headers is null) throw new Exception("Request headers can't be null, check the request");
    }
}