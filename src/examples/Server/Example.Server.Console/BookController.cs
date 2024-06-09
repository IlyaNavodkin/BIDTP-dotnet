using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Handle;
using BIDTP.Dotnet.Core.Iteraction.Routing.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Server.Console
{
    [ControllerRoute("book")]
    public class BookController : ControllerBase
    {
        private readonly ILogger logger;

        public BookController(ILogger logger) 
        {
            this.logger = logger;
        }

        [MethodRoute("sayHello")]
        public async Task BOOKSAYHELLO(Context context)
        {
            var request = context.Request;

            logger.LogInformation("APPLE");

            var result = "APPLE!!!";
            var response = new Response(StatusCode.Success);
            response.SetBody(result);

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
    }
}
