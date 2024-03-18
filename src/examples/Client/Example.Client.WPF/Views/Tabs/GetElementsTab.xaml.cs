using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Request;
using BIDTP.Dotnet.Core.Response.Dtos;
using BIDTP.Dotnet.Core.Response.Enums;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Newtonsoft.Json;

namespace Example.Client.WPF.Views.Tabs;

public partial class GetElementsTab : UserControl
{
    public GetElementsTab()
    {
        InitializeComponent();
    }

    private void InputTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private async void GetElements(object sender, RoutedEventArgs e)
    {
        try
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var token = mainWindow.AuthTokenTextBox.Text;
            
            var request = new Request();
        
            request.Headers.Add("Authorization", token);
            request.SetRoute("GetElements");

            var getCategoryRequest = new GetElementsByCategoryRequest
            {
                Category = CategoryTextBox.Text
            };
        
            var jsonResponse = JsonConvert.SerializeObject(getCategoryRequest);
            request.Body = jsonResponse;
            
            var response = await App.Client.WriteRequestAsync(request);
        
            if (response.StatusCode == StatusCode.Success)
            {
                var json = response.Body;
            
                var elements = JsonConvert.DeserializeObject<ICollection<ElementDto>>(json);
            
                DataGridElements.ItemsSource = elements;
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

    private async void DeleteElement(object sender, RoutedEventArgs e)
    {
        try
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var token = mainWindow.AuthTokenTextBox.Text;
            
            var request = new Request();
        
            var selectedItem = (ElementDto)DataGridElements.SelectedItem;
        
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
            
                foreach (var objectElement in DataGridElements.ItemsSource)
                {
                    var element = (ElementDto)objectElement;
                
                    if (element.Id == selectedItem.Id) continue;
                
                    newList.Add(element);
                }
            
                DataGridElements.ItemsSource = newList;

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
}