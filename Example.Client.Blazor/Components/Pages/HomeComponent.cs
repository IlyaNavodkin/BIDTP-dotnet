using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions;
using Example.Services.Generate;
using Microsoft.AspNetCore.Components;

namespace Example.Client.Blazor.Components.Pages
{
    public class HomeComponent : ComponentBase
    {
        [Inject] private IBidtpClient _bidtpClient { get; }

        public async Task CreateRandomWall()
        {
            try
            {
                var request = new Request();

                var body = RandomPointGenerateService.GeneratePointsWithMinDistance(0.3, 10);

                request.SetBody(body);
                request.SetRoute("CreateRandomWall");

                var response = await _bidtpClient.Send(request);
                var text = response.GetBody<string>();
            }
            catch (Exception ex)
            {
            }
        }

    }

}
