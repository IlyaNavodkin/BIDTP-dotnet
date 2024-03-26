// using BIDTP.Dotnet.Core.Iteraction.Interfaces;
// using BIDTP.Dotnet.Core.Iteraction.Providers;
// using Example.Schemas.Dtos;
// using Newtonsoft.Json;
//
// namespace Example.Server.Domain.Auth.Middlewares;
//
// public class MappingMiddlewareAttribute : Attribute, IMethodScopedPreInvokable
// {
//     private readonly Type _type;
//
//     public MappingMiddlewareAttribute(Type type)
//     {
//         _type = type;
//     }
//
//     public Task Invoke(Context context)
//     {
//         var request = context.Request;
//         var objectContainer = context.ObjectContainer;
//
//         var deserializedObject =  request.GetBody<_type>();
//
//         objectContainer.AddObject(_type, deserializedObject);
//
//         var additionalData = new AdditionalData
//         {
//             Guid = Guid.NewGuid().ToString(),
//             DateTime = DateTime.Now
//         };
//
//         objectContainer.AddObject<AdditionalData>(additionalData);
//
//         return Task.CompletedTask;
//     }
// }