using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Schema;
using Example.Client.Services;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Defaults.Classes;

namespace Example.Client.Blazor.Pages
{
    public partial class ElementRevitTab
    {
        [Inject]
        public IBidtpClient BidtpClient { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        private List<ElementDto> _elements = new();
        private ElementDto _selectedElement;
        private string _token = string.Empty;
        private string _categoryName = string.Empty;

        public async Task SelectedElementChanged(ElementDto element)
        {
            _selectedElement = element;

            Snackbar.Add($"Selected element with id: {element.Id}", Severity.Info);
        }

        private async Task GetElements()
        {
            try
            {
                var request = new Request();

                request.Headers.Add("Authorization", _token);
                request.SetRoute("ElementRevit/GetElements");

                var getCategoryRequest = new GetElementsByCategoryRequest
                {
                    Category = _categoryName
                };

                request.SetBody<GetElementsByCategoryRequest>(getCategoryRequest);

                var response = await BidtpClient.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    var elements = response.GetBody<List<ElementDto>>();

                    _elements = elements;

                    Snackbar.Add("All Revit elements have been fetched.", Severity.Success);
                }
                else
                {
                    var error = response.GetBody<string>();

                    Snackbar.Add(error, Severity.Error);
                }

            }
            catch (Exception ex) 
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }


        private async Task DeleteElement()
        {
            try
            {
                if (_selectedElement is null)
                {
                    Snackbar.Add("Please select an element to delete.", Severity.Error);

                    return;
                }

                var request = new Request();

                request.Headers.Add("Authorization", _token);
                request.SetRoute("ElementRevit/DeleteElement");

                var getCategoryRequest = new DeleteElementRequest
                {
                    Element = _selectedElement
                };

                request.SetBody<DeleteElementRequest>(getCategoryRequest);

                var response = await BidtpClient.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    _elements.Remove(_selectedElement);

                    Snackbar.Add("Element has been deleted.", Severity.Success);
                }
                else
                {
                    var error = response.GetBody<string>();

                    Snackbar.Add(error, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task CreateRandomWall()
        {
            try
            {
                var randomLine = RandomService.GenerateRandomLine(4);

                var request = new Request();

                request.Headers.Add("Authorization", _token);
                request.SetRoute("ElementRevit/CreateRandomWall");

                var getCategoryRequest = new CreateRandomWallLineRequest
                {
                    Line = randomLine
                };

                request.SetBody<CreateRandomWallLineRequest>(getCategoryRequest);

                var response = await BidtpClient.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    Snackbar.Add(response.GetBody<string>(), Severity.Success);
                }
                else
                {
                    var error = response.GetBody<BIDTPError>();

                    Snackbar.Add(error.Message, Severity.Error);
                }
            }
            catch (Exception exception)
            {
                Snackbar.Add(exception.Message, Severity.Error);
            }
        }

        private async Task CreateFloorsColumns()
        {
            try
            {
                var randomLine = RandomService.GenerateRandomLine(4);

                var request = new Request();

                request.Headers.Add("Authorization", _token);
                request.SetRoute("ElementRevit/CreateFloorsColumns");

                request.SetBody<string>("Can u please create some columns?");

                var response = await BidtpClient.Send(request);

                if (response.StatusCode == StatusCode.Success)
                {
                    var result = response.GetBody<string>();

                    Snackbar.Add(result, Severity.Success);
                }
                else
                {
                    var error = response.GetBody<string>();

                    Snackbar.Add(error, Severity.Error);
                }
            }
            catch (Exception exception)
            {
                Snackbar.Add(exception.Message, Severity.Error);
            }
        }
    }
}