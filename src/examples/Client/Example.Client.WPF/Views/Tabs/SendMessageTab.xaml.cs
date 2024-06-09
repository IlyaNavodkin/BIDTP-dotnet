using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Iteraction;
using UIFramework;

namespace Example.Client.WPF.Views.Tabs;

public partial class SendMessageTab : UserControl
{
    public SendMessageTab()
    {
        InitializeComponent();
    }

    private void SendMessageButton_OnClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        var token = mainWindow.AuthTokenTextBox.Text;

        try
        {
            var taskRun = Task.Run(async () =>
            {
                MainWindow? mainWindow = null;

                var request = new Request();

                var stringBuilder = new StringBuilder();

                request.SetBody(stringBuilder.ToString());

                request.Headers.Add("Authorization", token);
                request.SetRoute("Color/GetRandomColor");

                var response = await App.Client.Send(request);

                var formattedResponseText = response.GetBody<string>();

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