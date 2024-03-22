using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Example.Client.WPF.MVVM.Services;

namespace Example.Client.WPF.MVVM.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private string  _authToken;
    private CancellationTokenSource _cancelTokenSource;
    
    public MainWindowViewModel()
    {
        _authService = AuthService.GetInstance();
        
        ConnectToServerCommand = new AsyncRelayCommand(ConnectToServer);
    }
    
    public string AuthToken
    {
        get => _authToken;
        set
        {
            SetProperty(ref _authToken, value);
            
            _authService.SetAuthToken(value);
        }
    }
    
    private int  _chunkSize;

    public int ChunkSize
    {
        get => _chunkSize;
        set => SetProperty(ref _chunkSize, value);
    }

    private async Task ConnectToServer(CancellationToken cancellationToken)
    {
        try 
        {
            var isConnected = App.Client.IsHealthCheckConnected;
            
            if (!isConnected)
            { 
                if (App.Client.IsConnectionStarting) throw new Exception("Connection already started");
                    
                _cancelTokenSource = new CancellationTokenSource();
                await App.Client.ConnectToServer(_cancelTokenSource);
                
                if (!App.Client.IsHealthCheckConnected) throw new Exception("Connection failed");
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
    public IAsyncRelayCommand ConnectToServerCommand { get; set; }
}