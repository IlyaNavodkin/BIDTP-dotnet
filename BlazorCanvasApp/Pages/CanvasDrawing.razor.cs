using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Contracts;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Client.Blazor.Pages
{
    public partial class CanvasDrawing
    {
        ElementReference divCanvas;
        BECanvasComponent myCanvas;
        Canvas2DContext currentCanvasContext;
        List<Point> points = new List<Point>();
        List<Line> lines = new List<Line>();

        private enum Mode { None, DrawLine, DrawPoint, Move, Delete }
        private Mode currentMode = Mode.None;

        private Point startPoint = null;
        private bool isDragging = false;
        private Point selectedPoint = null;
        private Line selectedLine = null;
        private bool draggingStartPoint = false;

        [Inject]
        IBidtpClient bidtpClient { get; set; }

        private class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
            public string GUID { get; set; } = Guid.NewGuid().ToString();
            public double Radius { get; set; } = 10;

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private class Line
        {
            public Point Start { get; set; }
            public Point End { get; set; }
            public string GUID { get; set; } = Guid.NewGuid().ToString();
            public string ElementId { get; set; }


            public Line(Point start, Point end)
            {
                Start = start;
                End = end;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                currentCanvasContext = await myCanvas.CreateCanvas2DAsync();
            }
        }

        async void OnClick(MouseEventArgs eventArgs)
        {
            double mouseX, mouseY;
            (mouseX, mouseY) = await GetMouseCoordinates(eventArgs);

            if (currentMode == Mode.DrawPoint)
            {
                points.Add(new Point(mouseX, mouseY));
                await RenderCanvas();
            }
            else if (currentMode == Mode.DrawLine)
            {
                if (startPoint == null)
                {
                    startPoint = new Point(mouseX, mouseY);
                }
                else
                {
                    var endPoint = new Point(mouseX, mouseY);

                    var line = new Line(startPoint, endPoint);

                    lines.Add(line);
                    startPoint = null;

                    var request = new Request();

                    var dto = new WallLineRequest();

                    var lineDto = new LineDto();

                    var startPointDto = new PointDto { X = line.Start.X, Y = line.Start.Y };
                    var endPointDto = new PointDto { X = line.End.X, Y = line.End.Y };

                    lineDto.StartPoint = startPointDto;
                    lineDto.EndPoint = endPointDto;
                    lineDto.Guid = line.GUID;

                    dto.Line = lineDto;

                    request.SetBody(dto);

                    request.SetRoute("Element/CreateRandomWall");

                    var response = await bidtpClient.Send(request);

                    var statusCode = response.StatusCode;

                    if (statusCode is StatusCode.Success)
                    {
                        var elementIdString = response.GetBody<string>();

                        line.ElementId = elementIdString;
                    }
                    else
                    {
                        var error = response.GetBody<string>();

                        Console.WriteLine(error);
                    }

                    await RenderCanvas();
                }
            }
            else if (currentMode == Mode.Move)
            {
                // Deselect previously selected elements
                foreach (var point in points)
                {
                    point.GUID = Guid.NewGuid().ToString();
                }
                foreach (var line in lines)
                {
                    line.GUID = Guid.NewGuid().ToString();
                }

                // Select point or start dragging
                selectedPoint = FindPointUnderCursor(mouseX, mouseY);
                selectedLine = FindLineUnderCursor(mouseX, mouseY);
                if (selectedLine != null)
                {
                    GetNearestLinePoint(selectedLine, mouseX, mouseY);
                }
                await RenderCanvas();
            }
            else if (currentMode == Mode.Delete)
            {
                var line = FindLineUnderCursor(mouseX, mouseY);

                if (line is null) return;

                lines.Remove(line);

                var request = new Request();

                var dto = new WallRemoveRequest();

                dto.ElementId = line.ElementId;

                request.SetBody(dto);

                request.SetRoute("Element/RemoveWall");

                var response = await bidtpClient.Send(request);

                var statusCode = response.StatusCode;

                if (statusCode is StatusCode.Success)
                {
                    var message = response.GetBody<string>();

                    Console.WriteLine(message);
                }
                else
                {
                    var error = response.GetBody<string>();

                    Console.WriteLine(error);
                }

                await RenderCanvas();
            }
        }

        async Task<(double, double)> GetMouseCoordinates(MouseEventArgs eventArgs)
        {
            string data = await jsRuntime.InvokeAsync<string>("getDivCanvasOffsets", new object[] { divCanvas });
            JObject offsets = (JObject)JsonConvert.DeserializeObject(data);
            double mouseX = eventArgs.ClientX - offsets.Value<double>("offsetLeft");
            double mouseY = eventArgs.ClientY - offsets.Value<double>("offsetTop");
            return (mouseX, mouseY);
        }

        async Task RenderCanvas()
        {
            await currentCanvasContext.ClearRectAsync(0, 0, 800, 800);

            // ��������� �����
            foreach (var point in points)
            {
                await currentCanvasContext.SetFillStyleAsync(point.GUID == selectedPoint?.GUID ? "Green" : "Red");
                await currentCanvasContext.FillRectAsync(point.X, point.Y, point.Radius, point.Radius);
            }

            // ��������� �����
            await currentCanvasContext.SetLineWidthAsync(2);
            foreach (var line in lines)
            {
                await currentCanvasContext.SetStrokeStyleAsync(line.GUID == selectedLine?.GUID ? "Green" : "Blue");
                await currentCanvasContext.BeginPathAsync();
                await currentCanvasContext.MoveToAsync(line.Start.X, line.Start.Y);
                await currentCanvasContext.LineToAsync(line.End.X, line.End.Y);
                await currentCanvasContext.StrokeAsync();

                // ��������� ������ �����
                await currentCanvasContext.SetFillStyleAsync(line.Start.GUID == selectedPoint?.GUID ? "Green" : "Black");
                await currentCanvasContext.FillRectAsync(line.Start.X - 3, line.Start.Y - 3, line.Start.Radius, line.Start.Radius);

                await currentCanvasContext.SetFillStyleAsync(line.End.GUID == selectedPoint?.GUID ? "Green" : "Black");
                await currentCanvasContext.FillRectAsync(line.End.X - 3, line.End.Y - 3, line.End.Radius, line.End.Radius);
            }

            await currentCanvasContext.SetFontAsync("16px Arial");
            await currentCanvasContext.SetFillStyleAsync("Black");
            if (points.Count > 0)
            {
                var lastPoint = points[^1];
                await currentCanvasContext.FillTextAsync($"ClientX: {lastPoint.X}   ClientY: {lastPoint.Y}", 20, 20);
            }

        }
        void EnableLineDrawingMode()
        {
            currentMode = Mode.DrawLine;
        }

        void EnablePointDrawingMode()
        {
            currentMode = Mode.DrawPoint;
        }

        void EnableMoveMode()
        {
            currentMode = Mode.Move;
        }

        void EnableDeleteMode()
        {
            currentMode = Mode.Delete;
        }

        Point FindPointUnderCursor(double x, double y)
        {
            const double tolerance = 10;
            return points.FirstOrDefault(p => Math.Abs(p.X - x) <= tolerance && Math.Abs(p.Y - y) <= tolerance);
        }

        Line FindLineUnderCursor(double x, double y)
        {
            const double tolerance = 5;
            foreach (var line in lines)
            {
                if ((Math.Abs(line.Start.X - x) <= tolerance && Math.Abs(line.Start.Y - y) <= tolerance) ||
                    (Math.Abs(line.End.X - x) <= tolerance && Math.Abs(line.End.Y - y) <= tolerance))
                {
                    return line;
                }
            }
            return null;
        }

        void GetNearestLinePoint(Line line, double x, double y)
        {
            var startDistance = Math.Sqrt(Math.Pow(line.Start.X - x, 2) + Math.Pow(line.Start.Y - y, 2));
            var endDistance = Math.Sqrt(Math.Pow(line.End.X - x, 2) + Math.Pow(line.End.Y - y, 2));
            draggingStartPoint = startDistance < endDistance;
        }

        async void OnMouseMove(MouseEventArgs eventArgs)
        {
            if (currentMode == Mode.Move && isDragging)
            {
                double mouseX, mouseY;
                (mouseX, mouseY) = await GetMouseCoordinates(eventArgs);

                if (selectedPoint != null)
                {
                    selectedPoint.X = mouseX;
                    selectedPoint.Y = mouseY;
                }
                else if (selectedLine != null)
                {
                    if (draggingStartPoint)
                    {
                        selectedLine.Start.X = mouseX;
                        selectedLine.Start.Y = mouseY;
                    }
                    else
                    {
                        selectedLine.End.X = mouseX;
                        selectedLine.End.Y = mouseY;
                    }
                }

                var line = FindLineUnderCursor(mouseX, mouseY);
                if (line != null)
                {
                    GetNearestLinePoint(line, mouseX, mouseY);
                }
                else
                {
                    draggingStartPoint = false;
                }

                var request = new Request();

                var dto = new WallLineRequest();

                var lineDto = new LineDto();

                var startPointDto = new PointDto { X = line.Start.X, Y = line.Start.Y };
                var endPointDto = new PointDto { X = line.End.X, Y = line.End.Y };

                lineDto.StartPoint = startPointDto;
                lineDto.EndPoint = endPointDto;
                lineDto.Guid = line.GUID;
                lineDto.ElementId = line.ElementId;

                dto.Line = lineDto;

                request.SetBody(dto);

                request.SetRoute("Element/ChangeWallLocation");

                var response = await bidtpClient.Send(request);

                await RenderCanvas();
            }
        }

        void OnMouseDown(MouseEventArgs eventArgs)
        {
            if (currentMode == Mode.Move && (selectedPoint != null || selectedLine != null))
            {
                isDragging = true;
            }
        }

        void OnMouseUp(MouseEventArgs eventArgs)
        {
            isDragging = false;
            selectedPoint = null;
            selectedLine = null;
        }

        void ShowPoints()
        {
            StateHasChanged();
        }
    }
}