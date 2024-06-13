#

Nuget - https://www.nuget.org/packages/BIDTP.Dotnet.Core

# Preface

BIDTP-dotnet - this is library for implementing client-server architecture for inter-process communication based on BIDTP protocol for dotnet.

Bidirectional Interprocess Data Transfer Protocol (BIDTP) - a simple-to-use data exchange protocol between two processes.

[References diagram](https://github.com/IlyaNavodkin/BIDTP-dotnet/blob/master/Common/images/BIDTP-dotnet-references-schema-1.png)

# Questions and answers

### Ask:

-   Why should I split my application into client and server?

### Answer:

-   Separate testing and debugging of applications. It's easier to abstract yourself
-   Independence from stack and technologies
-   Separate process, domain for each application. If your application works as a plugin, then this solves the problem of dependency conflicts (Addin Autodesk Revit)
-   Separation of areas of responsibility for application development - if it is developed in a team

### Ask:

-   Why should I use this protocol if I can use web protocols?

### Answer:

-   This protocol is intended for local tasks of interaction between two processes. If you use web protocols for such tasks, you may encounter unexpected problems on the client side. For example - the client’s port is closed or there is a problem with the network

### Ask:

-   It's too complicated, I'll have to write a lot of code to interact with the business logic. It's easier to call a service or method directly within one process

### Answer:

-   The protocol is provided with the entire infrastructure with features that allow not only to minimize the cost of writing code for interaction, but also provide general functionality - necessary for creating modern applications - on the client and server sides

# Philosophy and goals

The main issues addressed by this protocol include facilitating simple interaction between two processes. This enables independent and implicit communication between applications, regardless of the technologies used in their development.

The same effect can be achieved by using web protocols such as WebSocket, TCP, UDP, HTTP, and others. However, their obvious downside is that for their correct operation, it is necessary to ensure that the required ports are open on the local machine of the client or server and there are no issues with the network infrastructure.

In the current approach, data exchange between processes occurs at a lower level (byte-to-byte).

## Acknowledgements

I express my deep gratitude to [Nice3point](https://github.com/Nice3point) (Roman Karpovich), particularly for his article on process communication:
https://github.com/atomatiq/InterprocessCommunication

# Structure of BIDTP protocol

Below are listed the main components along with their general descriptions.

## Interaction scheme

The BIDTP protocol is based on client-server interaction. Communication between the components occurs using messages in a byte stream (pipe stream). The messages consist of Request\Response objects.

## Requirements to request

The request object has the following structure:

| Field name     | Type        | Size (bytes) |
| -------------- | ----------- | ------------ |
| MessageLength  | int         | 4            |
| HeadersLength  | int         | 4            |
| HeadersContent | string      | var          |
| BodyLength     | int         | 4            |
| BodyContent    | string      | var          |

## Requirements to response

The response object has the following structure:

| Field name     | Type        | Size (bytes) |
| -------------- | ----------- | ------------ |
| MessageLength  | int         | 4            |
| StatusCode     | enum (int ) | 4            |
| HeadersLength  | int         | 4            |
| HeadersContent | string      | var          |
| BodyLength     | int         | 4            |
| BodyContent    | string      | var          |

## Status code

The status code is a specific code (classification) of responses from the server according to this protocol. The main status codes and their descriptions are provided below:

| Code         | Description                                                       |
| ------------ | ----------------------------------------------------------------- |
| Success      | 100 - Successful response from the server                         |
| ClientError  | 200 - Not successful Server response - error on client side       |
| Unauthorized | 201 - Not successful Server response - client is not unauthorized |
| NotFound     | 202 - Not successful Server response - route not found            |
| ServerError  | 300 - Not successful Server response - error on server side       |


# Examples

In the repository, there are examples of .NET applications:

1. Example.Client.Blazor - A Blazor client application based on the .NET Core
2. Example.Client.Console - A console client application based on the .NET Core
3. Example.Client.WPF -  A WPF client application based on the .NET Core
4. Example.Server.Console - A server console application based on the .NET Framework 4.8
5. Example.Server.Console.OwnerProcess - A server console application based on the .NET Framework 4.8, which runs another client-child process. U can build and run Example.Client.WPF.
6. Example.Server.Revit - A server application based on the .NET Framework 4.8 and Revit API
7. Example.Schemas - Declaration of generic DTO schemas for client-server communication.

More examples in BIDTP.Dotnet.Tests.Integration
