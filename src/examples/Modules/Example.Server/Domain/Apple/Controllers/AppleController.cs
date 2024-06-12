using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Attributes;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using Example.Server.Domain.Auth.Middlewares;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Server.Domain.Apple.Controllers
{
    [ControllerRoute("apple")]
    public class AppleController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly string Id = Guid.NewGuid().ToString();

        public AppleController(ILogger logger)
        {
            this.logger = logger;
        }

        [MethodRoute("sayHello")]
        [FirstCustomMiddleware]
        [SecondCustomMiddleware]
        public async Task SAYHELLO(Context context)
        {
            var request = context.Request;

            logger.LogInformation("Hello");

            var result = "Hello!!!";
            var response = new Response(StatusCode.Success);
            response.SetBody(result);

            logger.LogWarning(Id);

            context.Response = response;
        }

        [MethodRoute("sayFuckU")]
        public async Task FUCKU(Context context)
        {
            var request = context.Request;

            logger.LogInformation("Fuck you");

            var result = "Fuck you!!!";
            var response = new Response(StatusCode.Success);
            response.SetBody(result);

            context.Response = response;
        }

        [MethodRoute("throwException")]
        public async Task TrowException(Context context)
        {
            throw new Exception("TEST EXCEPTION");
        }
    }
}
