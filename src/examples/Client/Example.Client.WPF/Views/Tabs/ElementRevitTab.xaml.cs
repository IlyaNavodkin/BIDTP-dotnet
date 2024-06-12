using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Schema;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Example.Server.Domain.Elements.Services;

namespace Example.Client.WPF.Views.Tabs;

public partial class ElementRevitTab : UserControl
{
    public ElementRevitTab()
    {
        InitializeComponent();
    }

    private void GetElements(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        var token = mainWindow.AuthTokenTextBox.Text;
        var categoryName = CategoryTextBox.Text;

        var taskRun = Task.Run(async () =>
        {
            try
            {
                var request = new Request();

                request.Headers.Add("Authorization", token);
                request.SetRoute("ElementRevit/GetElements");

                var getCategoryRequest = new GetElementsByCategoryRequest
                {
                    Category = categoryName
                };

                request.SetBody<GetElementsByCategoryRequest>(getCategoryRequest);

                var response = await App.Client.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    var elements = response.GetBody<List<ElementDto>>();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DataGridElements.ItemsSource = elements;
                    });
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
        });
    }

    private void DeleteElement(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        var token = mainWindow.AuthTokenTextBox.Text;
        var elementForDelete = (ElementDto)DataGridElements.SelectedItem;

        if (elementForDelete is null)
        {
            MessageBox.Show("Вы не выбрали элемент для удаления");

            return;
        }

        var taskRun = Task.Run(async () =>
        {
            try
            {
                var request = new Request();

                request.Headers.Add("Authorization", token);
                request.SetRoute("ElementRevit/DeleteElement");

                var getCategoryRequest = new DeleteElementRequest
                {
                    Element = elementForDelete
                };

                request.SetBody<DeleteElementRequest>(getCategoryRequest);

                var response = await App.Client.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var itemsSource = DataGridElements.ItemsSource as IList<ElementDto>;
                        if (itemsSource != null)
                        {
                            itemsSource.Remove(elementForDelete);
                            DataGridElements.Items.Refresh();
                        }
                    });
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
        });
    }

    private void CreateRandomWall(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        var token = mainWindow.AuthTokenTextBox.Text;

        var taskRun = Task.Run(async () =>
        {
            try
            {
                var randomLine = RandomService.GenerateRandomLine(2);

                var request = new Request();

                request.Headers.Add("Authorization", token);
                request.SetRoute("ElementRevit/CreateRandomWall");

                var getCategoryRequest = new CreateRandomWallLineRequest
                {
                    Line = randomLine
                };

                request.SetBody<CreateRandomWallLineRequest>(getCategoryRequest);

                var response = await App.Client.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    var result = response.GetBody<string>();

                    MessageBox.Show(result);
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
        });
    }
}