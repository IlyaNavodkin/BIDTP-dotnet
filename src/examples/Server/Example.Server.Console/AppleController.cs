using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Server.Console
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

        [Route("sayHello")]
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

        [Route("sayFuckU")]
        public async Task FUCKU(Context context)
        {
            var request = context.Request;

            logger.LogInformation("Fuck you");

            var result = "Fuck you!!!";
            var response = new Response(StatusCode.Success);
            response.SetBody(result);

            context.Response = response;
        }

        [Route("throwException")]
        public async Task TrowException(Context context)
        {
            throw new Exception("TEST EXCEPTION");
        }
    }
}
