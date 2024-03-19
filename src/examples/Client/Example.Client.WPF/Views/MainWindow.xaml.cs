using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using BIDTP.Dotnet.Events;

namespace Example.Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CancellationTokenSource _cancelTokenSource;

        /// <summary>
        ///  Client window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            ConnectToServerButton.Click += ConnectToServer;

            App.Client.IsLifeCheckConnectedChanged += (s, e) =>
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
            
            App.Client.ReadProgress += OnProgress;
            App.Client.WriteProgress += OnProgress;
        }
        
        private async void OnProgress(object? sender, ProgressEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var bytesWritten = e.BytesWritten;
                var totalBytes = e.TotalBytes; 
                var progressPercentage = (int)((double)bytesWritten / totalBytes * 100);

                if (totalBytes == bytesWritten)
                {
                    ProgressBarTextBlock.Text = "Completed";
                    ProgressBar.Value = 0;
                }
                else if (progressPercentage >= 0 && progressPercentage <= 100)
                {
                    var message = $"{e.ProgressOperationType.ToString()} progress: {progressPercentage}%";
                    
                    Debug.WriteLine(message);

                    ProgressBarTextBlock.Text = message;
                    ProgressBar.Value = progressPercentage;
                }
            });
        }

        private async void ConnectToServer(object sender, RoutedEventArgs e)
        {
            try
            {
                var isConnected = App.Client.IsHealthCheckConnected;
            
                if (!isConnected)
                { 
                    if (App.Client.IsConnectionStarting) throw new Exception("Connection already started");
                    
                    _cancelTokenSource = new CancellationTokenSource();
                    await App.Client.ConnectToServer(_cancelTokenSource);
                }
                else
                {
                    _cancelTokenSource.Cancel();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}