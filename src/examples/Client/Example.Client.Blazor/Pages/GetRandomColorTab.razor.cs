using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Example.Client.Blazor.Pages
{
    public partial class GetRandomColorTab
    {
        [Inject]
        public IBidtpClient BidtpClient { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        private string _token = string.Empty;

        private async Task GetRandomColor()
        {
            try
            {
                var request = new Request();

                request.Headers.Add("Authorization", _token);
                request.SetRoute("Color/GetRandomColor");

                request.SetBody<string>("");

                var response = await BidtpClient.Send(request);

                var formattedResponseText = response.GetBody<string>();

                if (response.StatusCode == StatusCode.Success)
                {
                    Snackbar.Add(formattedResponseText, Severity.Success);
                }
                else
                {
                    Snackbar.Add(formattedResponseText, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}