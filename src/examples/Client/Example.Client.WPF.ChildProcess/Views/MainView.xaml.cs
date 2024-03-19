using System;
using System.Collections.Generic;
using System.Windows;
using BIDTP.Dotnet.Iteraction.Request;
using BIDTP.Dotnet.Iteraction.Response.Dtos;
using BIDTP.Dotnet.Iteraction.Response.Enums;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Newtonsoft.Json;

namespace Example.Client.WPF.ChildProcess.Views;

/// <summary>
///  Interaction logic for MainView.xaml
/// </summary>
public sealed partial class MainView
{
    /// <summary>
    ///  Initialize a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        RequestCheckButton.Click += async (sender, args) =>
        {
            try
            {
                var message = MessageTextBox.Text;
                var request = new Request
                {
                    Body = message
                };

                request.Headers.Add("Authorization", TokenTextBox.Text);
                request.SetRoute("PrintMessage");

                var response = await App.Client.WriteRequestAsync(request);

                if (response.StatusCode is StatusCode.Success)
                {
                    TextBlockResponse.Text = response.Body;
                }
                else
                {
                    var error = JsonConvert.DeserializeObject<Error>(response.Body);

                    MessageBox.Show($"Message: {error.Message} \nError code: {error.ErrorCode}\n Description: {error.Description}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        };
    }

    private async void GetElements(object sender, RoutedEventArgs e)
    {
        try
        {
            var request = new Request();

            request.Headers.Add("Authorization", TokenTextBox.Text);
            request.SetRoute("GetElements");

            var getCategoryRequest = new GetElementsByCategoryRequest
            {
                Category = CategoryTextBox.Text
            };

            var jsonResponse = JsonConvert.SerializeObject(getCategoryRequest);
            request.Body = jsonResponse;

            WindowState = WindowState.Minimized;

            var response = await App.Client.WriteRequestAsync(request);

            WindowState = WindowState.Normal;

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
            var request = new Request();

            var selectedItem = (ElementDto)DataGridElements.SelectedItem;

            request.Headers.Add("Authorization", TokenTextBox.Text);
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