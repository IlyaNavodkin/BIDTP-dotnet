using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Example.Client.WPF.Views.Tabs;

public partial class SendMessageTab : UserControl
{
    public SendMessageTab()
    {
        InitializeComponent();
    }

    private async void SendMessageButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
        
            var request = new Request
            {
                Body = MessageInputTextBox.Text,
                Headers = new Dictionary<string, string>()
            };
        
            var token = mainWindow.AuthTokenTextBox.Text;
        
            request.Headers.Add("Authorization", token);
            request.SetRoute("PrintMessage");
        
            var response = await App.Client.WriteRequestAsync(request);
        
            var formattedResponseText = JToken.Parse(response.Body)
                .ToString(Newtonsoft.Json.Formatting.Indented);

            var responseFullJson = JsonConvert.SerializeObject(response);
            var requestFullJson = JsonConvert.SerializeObject(request);
            
            OutPutTextBlock.Text = formattedResponseText;
    
            MessageBox.Show(formattedResponseText);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }
}