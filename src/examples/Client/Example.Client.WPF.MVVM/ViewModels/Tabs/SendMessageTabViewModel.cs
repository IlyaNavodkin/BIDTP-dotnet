using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Example.Client.WPF.MVVM.Services;

namespace Example.Client.WPF.MVVM.ViewModels.Tabs;

public class SendMessageTabViewModel : ObservableObject
{
    private readonly AuthService _authService;
    public SendMessageTabViewModel()
    {
        _authService = AuthService.GetInstance();
        
        SendMessageCommand = new AsyncRelayCommand(SendMessage);
    }

    private string  _inputMessage;
    public string InputMessage
    {
        get => _inputMessage;
        set => SetProperty(ref _inputMessage, value);
    }
    
    private string  _outputMessage;

    public string OutputMessage
    {
        get => _outputMessage;
        set => SetProperty(ref _outputMessage, value);
    }
    
    private async Task SendMessage(CancellationToken cancellationToken)
    {
        try
        {
            var inputMessage = InputMessage;
            
            var request = new Request ();
            
            request.SetBody(inputMessage);
            
            var token = _authService.GetAuthToken();
        
            request.Headers.Add("Authorization", token);
            request.SetRoute("PrintMessage");
        
            var response = await App.Client.WriteRequestAsync(request);
            
            OutputMessage = response.GetBody<string>();
    
            MessageBox.Show(OutputMessage);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }
    
    public IAsyncRelayCommand SendMessageCommand { get; set; }
}