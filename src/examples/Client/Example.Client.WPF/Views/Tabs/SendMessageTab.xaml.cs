using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Iteraction;

namespace Example.Client.WPF.Views.Tabs;

public partial class SendMessageTab : UserControl
{
    public SendMessageTab()
    {
        InitializeComponent();
    }

    private void SendMessageButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var taskRun = Task.Run(async () =>
            {
                MainWindow? mainWindow = null;

                var request = new Request();

                string? multipleValueString = null;
                string? messageValue = null;
                string? token = null;

                var multilpleValue = 0;

                if (multipleValueString is null || !int.TryParse(multipleValueString, out multilpleValue))
                {
                    multilpleValue = 1;
                }

                var stringBuilder = new StringBuilder();

                for (var i = 0; i < multilpleValue; i++)
                {
                    stringBuilder.Append(messageValue);
                }

                request.SetBody(stringBuilder.ToString());

                request.Headers.Add("Authorization", token);
                request.SetRoute("PrintMessage");

                var response = await App.Client.Send(request);

                var formattedResponseText = response.GetBody<string>();

                //await Dispatcher.InvokeAsync(() =>
                //{
                //    SendMessageButton.IsEnabled = true;
                //    OutPutTextBlock.Text = formattedResponseText;
                //});

                MessageBox.Show(formattedResponseText);
            });

        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
            SendMessageButton.IsEnabled = true;
        }
    }
}