using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Schema;
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
            request.SetRoute("ElementRevit/GetElements");

            var getCategoryRequest = new GetElementsByCategoryRequest
            {
                Category = CategoryTextBox.Text
            };
        
            request.SetBody<GetElementsByCategoryRequest>(getCategoryRequest);
            
            var response = await App.Client.Send(request);
        
            if (response.StatusCode == StatusCode.Success)
            {
                var elements = response.GetBody<List<ElementDto>>();
            
                DataGridElements.ItemsSource = elements;
            }
            else
            {
                var error = response.GetBody<BIDTPError>();
            
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