using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Piders.Dotnet.Client;
using Piders.Dotnet.Core.Request;

namespace TestClient;

public partial class TestWindow : Window
{
    private readonly Client _client;
    private CancellationTokenSource cancelTokenSource;

    public TestWindow()
    {
        InitializeComponent();
        
        
        var options = new ClientOptions("testpipe", 1024, 9000, 
            1000, 5000);
        _client = new Client(options);
        cancelTokenSource = new CancellationTokenSource();
            
        // SendMessageButton.Click += OnRequestCheckButtonOnClick;
        
        _client.IsLifeCheckConnectedChanged += (s, e) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (e)
                {
                    EllipseStatusCircle.Fill = Brushes.Green;
                    ConnectToServerButton.Content = "Disconnect";
                    ServerStatusTextBlock.Text = "Server connected";
                }
                else
                {
                    EllipseStatusCircle.Fill = Brushes.Red;
                    ConnectToServerButton.Content = "Connect";
                    ServerStatusTextBlock.Text = "Server not connected";
        
                }
            });
        };

    }
    
    private async void ConnectToServer(object sender, RoutedEventArgs e)
    {
        var isConnected = _client.IsHealthCheckConnected;
            
        if (!isConnected)
        {
            cancelTokenSource = new CancellationTokenSource();
            await _client.ConnectToServer(cancelTokenSource);
        }
        else
        {
            cancelTokenSource.Cancel();
        }
    }

    private async void OnRequestCheckButtonOnClick(object sender, RoutedEventArgs args)
    {
        try
        {
            var request = new Request
            {
                Body = MessageTextBox.Text,
                Headers = new Dictionary<string, string>()
            };
            
            request.Headers.Add("Authorization", TokenTextBox.Text);
                
            request.SetRoute("PrintMessage"); 
                
            var response = await _client.WriteRequestAsync(request);
                
            var responseString = JsonConvert.SerializeObject(response);
                
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(responseString);
            });
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

}