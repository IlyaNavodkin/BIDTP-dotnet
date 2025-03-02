using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Example.Server.Revit;
using Example.Server.Revit.Constants;
using Example.Server.Revit.Extensions;
using Example.Server.Revit.Helpers;
using Example.Server.Revit.Stores;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using static Example.Server.Revit.Constants.ModulesConstants;

namespace Example.Modules.Server.Revit.ExternalCommands;

/// <summary>
/// Запускает внешний UI-процесс в модальном режиме
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class ModalRunAboutProgramExternalCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var services = SimpleDimpleExternalApplication.BidtpServer.Services;
            var store = services.GetRequiredService<ExecuteModuleStateStores>();

            if (store.IsRunning(ElementSelector.Name))
            {
                MessageBox.Show($"UI process of {nameof(ModalRunAboutProgramExternalCommand)} is already running");
                return Result.Cancelled;
            }

            var server = SimpleDimpleExternalApplication.BidtpServer
                         ?? throw new NullReferenceException("BidtpServer is null");


            var state = store.StartModule(ElementSelector.Name,
                ElementSelector.RelativePath, ServerApplicationConstants.PipeServerName);

            OsUiHelpers.DisableRevitUI();
            state.Process.WaitForProcessExit();
            OsUiHelpers.EnableRevitUI();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            OsUiHelpers.EnableRevitUI();

            TaskDialog.Show("Error", ex.Message);

            return Result.Failed;
        }
    }


}
