using System;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Example.Modules.Server.Revit.ExternalCommands;
using Example.Server.Revit.Utils;

namespace Example.Server.Revit.Configurator;

public class RevitUiConfigurator
{
    private static readonly Lazy<RevitUiConfigurator> Instance =
        new Lazy<RevitUiConfigurator>(() => new RevitUiConfigurator());

    public static RevitUiConfigurator GetInstance() => Instance.Value;

    public RibbonTab ConfigureRevitUiComponents(UIControlledApplication uiControlledApplication)
    {
        var uiService = UiComponentCreateUtil.GetInstance();

        var bimdataTab = uiService.CreateRibbonTab(uiControlledApplication, "BIDTP");

        var uploadPanel = uiService.CreateRibbonPanel(uiControlledApplication, bimdataTab,
            "BIDTP_PN1",
            "Выбрать файл WPF клиента");

        var uploadModelButton = uiService
            .CreatePushButton<RunAboutProgramExternalCommand>
            (
                "Выбрать файл WPF клиента",
                null,
                null,
                "Выбирает файл WPF клиента и запускает его - передавая необходимые аргументы"
            );

        uploadPanel.AddItem(uploadModelButton);

        return bimdataTab;
    }
}