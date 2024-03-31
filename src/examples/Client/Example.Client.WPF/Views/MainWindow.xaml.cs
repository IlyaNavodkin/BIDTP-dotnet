using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BIDTP.Dotnet.Core.Iteraction.Events;

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

            App.Client.IsLifeCheckConnectedChanged += async (s, e) =>
            {
                await Dispatcher.InvokeAsync(() =>
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
            var bytesWritten = e.BytesWritten;
            var totalBytes = e.TotalBytes; 
            var progressPercentage = (int)((double)bytesWritten / totalBytes * 100);
    
            if (progressPercentage % 5 != 0) return;
            
            var message = $"{e.ProgressOperationType.ToString()} progress: {progressPercentage}%";
            
            Debug.WriteLine(message);
    
            await Dispatcher.InvokeAsync(() =>
            {
                if (totalBytes == bytesWritten)
                {
                    ProgressBarTextBlock.Text = "Completed";
                    ProgressBar.Value = 0;
                }
                else if (progressPercentage >= 0 && progressPercentage <= 100)
                {
                    ProgressBarTextBlock.Text = message;
                    ProgressBar.Value = progressPercentage;
                }
            }, System.Windows.Threading.DispatcherPriority.Background); // Указываем приоритет Dispatcher
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

        private void ChunkSizeTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(ChunkSizeTextBox.Text, out var chunkSize))
            {
                if (chunkSize == 0 || chunkSize < 0) return;
                
                App.Client.ChunkSize = chunkSize;
            }
        }
    }
}