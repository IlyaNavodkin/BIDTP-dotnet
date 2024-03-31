using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;

namespace Example.Client.WPF.Views.Tabs;

/// <summary>
///  Interaction logic for GetElementsTab.xaml
/// </summary>
public partial class GetElementsTab : UserControl
{
    /// <summary>
    ///  Initialize a new instance of the <see cref="GetElementsTab"/> class.
    /// </summary>
    public GetElementsTab()
    {
        InitializeComponent();
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
        
            request.SetBody<GetElementsByCategoryRequest>(getCategoryRequest);
            
            var response = await App.Client.WriteRequestAsync(request);
        
            if (response.StatusCode == StatusCode.Success)
            {
                var elements = response.GetBody<List<ElementDto>>();
            
                DataGridElements.ItemsSource = elements;
            }
            else
            {
                var error = response.GetBody<Error>();
            
                MessageBox.Show($"Message: {error.Message} " +
                                $"\nError code: {error.ErrorCode}\nDescription: {error.Description}");
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
        
            request.SetBody<DeleteElementRequest>(deleteElementRequest);
            
            var response = await App.Client.WriteRequestAsync(request);
            if (response.StatusCode == StatusCode.Success)
            {
                var message = response.GetBody<string>();

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
                var error = response.GetBody<Error>();
            
                MessageBox.Show($"Message: {error.Message} " +
                                $"\nError code: {error.ErrorCode}\nDescription: {error.Description}");
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    private async void TestJson(object sender, RoutedEventArgs e)
    {
        try
        {
            var simpleObject = new AdditionalData
            {
                Guid = Guid.NewGuid().ToString(),
                Items = new List<string>  { "Item1", "Item2" },
                Name = "Test"
            };

            var request = new Request();
            
            request.SetRoute("GetMappedObjectFromObjectContainer");
            request.SetBody<AdditionalData>(simpleObject);
            
            var response = await App.Client.WriteRequestAsync(request);

            if (response.StatusCode is StatusCode.Success)
            {
                var dto = response.GetBody<AdditionalData>();
                
                var jsonStringDto = response.GetBody<string>();

                MessageBox.Show(jsonStringDto);
            }
            else
            {
                var error = response.GetBody<Error>();
            
                MessageBox.Show($"Message: {error.Message} " +
                                $"\nError code: {error.ErrorCode}\nDescription: {error.Description}");
            }

        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }
}