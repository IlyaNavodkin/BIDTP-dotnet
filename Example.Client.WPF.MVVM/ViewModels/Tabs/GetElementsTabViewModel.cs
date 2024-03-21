using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BIDTP.Dotnet.Iteraction;
using BIDTP.Dotnet.Iteraction.Dtos;
using BIDTP.Dotnet.Iteraction.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Example.Client.WPF.MVVM.Services;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Newtonsoft.Json;

namespace Example.Client.WPF.MVVM.ViewModels.Tabs;

public class GetElementsTabViewModel : ObservableObject
{
    private readonly AuthService _authService;
    
    public GetElementsTabViewModel()
    {
        _authService = AuthService.GetInstance();
        
        GetElementsCommand = new AsyncRelayCommand(GetElements);
        DeleteElementCommand = new AsyncRelayCommand(DeleteElement);
    }
    
    private string  _category;
    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }
    
    private ElementDto  _selectedElement;
    public ElementDto SelectedElement
    {
        get => _selectedElement;
        set => SetProperty(ref _selectedElement, value);
    }
    
    private ObservableCollection<ElementDto> _elements;
    
    public ObservableCollection<ElementDto> Elements
    {
        get => _elements;
        set => SetProperty(ref _elements, value);
    }
    
    private async Task DeleteElement(CancellationToken cancellationToken)
    {
        try
        {
            var token = _authService.GetAuthToken();
            
            var request = new Request();
        
            var selectedItem = SelectedElement;
        
            request.Headers.Add("Authorization", token);
            request.SetRoute("DeleteElement");

            var deleteElementRequest = new DeleteElementRequest
            {
                Element = selectedItem
            };
        
            var jsonResponse = JsonConvert.SerializeObject(deleteElementRequest);
            request.Body = jsonResponse;
            
            var response = await App.Client.WriteRequestAsync(request);
            if (response.StatusCode == StatusCode.Success)
            {
                var message = response.Body;

                var newList = new List<ElementDto>();
            
                foreach (var element in Elements)
                {
                    if (element.Id == selectedItem.Id) continue;
                
                    newList.Add(element);
                }
            
                Elements = new ObservableCollection<ElementDto>(newList);

                MessageBox.Show(message);
            }
            else
            {
                var error = JsonConvert.DeserializeObject<Error>(response.Body);
            
                MessageBox.Show($"Message: {error.Message} \nError code: {error.ErrorCode}\nDescription: {error.Description}");
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    private async Task GetElements(CancellationToken cancellationToken)
    {
        try
        {
            var token = _authService.GetAuthToken();
            
            var category = Category;
            
            var request = new Request();
        
            request.Headers.Add("Authorization", token);
            request.SetRoute("GetElements");

            var getCategoryRequest = new GetElementsByCategoryRequest
            {
                Category = category
            };
        
            var jsonResponse = JsonConvert.SerializeObject(getCategoryRequest);
            request.Body = jsonResponse;
            
            var response = await App.Client.WriteRequestAsync(request);
        
            if (response.StatusCode == StatusCode.Success)
            {
                var json = response.Body;
            
                var elements = JsonConvert.DeserializeObject<ICollection<ElementDto>>(json);
            
                Elements = new ObservableCollection<ElementDto>(elements);
            }
            else
            {
                var error = JsonConvert.DeserializeObject<Error>(response.Body);
            
                MessageBox.Show($"Message: {error.Message} \nError code: {error.ErrorCode}\nDescription: {error.Description}");
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    public IAsyncRelayCommand GetElementsCommand { get; set; }
    public IAsyncRelayCommand DeleteElementCommand { get; set; }
}