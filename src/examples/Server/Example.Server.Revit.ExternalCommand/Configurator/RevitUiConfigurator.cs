using System;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Example.Server.Revit.ExternalCommand.ExternalCommands;
using Example.Server.Revit.ExternalCommand.Utils;

namespace Example.Server.Revit.ExternalCommand.Configurator;

/// <summary>
///  The revit ui configurator 
/// </summary>
public class RevitUiConfigurator
{
    private static readonly Lazy<RevitUiConfigurator> Instance = 
        new Lazy<RevitUiConfigurator>(() => new RevitUiConfigurator());
    
    /// <summary>
    ///  Returns the instance of the singleton
    /// </summary>
    /// <returns> Singleton instance. </returns>
    public static RevitUiConfigurator GetInstance () => Instance.Value;
    
    /// <summary>
    /// Configures the revit ui components.
    /// </summary>
    /// <param name="uiControlledApplication"></param>
    /// <returns> Ribbon tab.</returns>
    public RibbonTab ConfigureRevitUiComponents(UIControlledApplication uiControlledApplication)
    {
        var uiService = UiComponentCreateUtil.GetInstance(); 
            
        var bimdataTab = uiService.CreateRibbonTab( uiControlledApplication,"BIDTP");
        
        var uploadPanel = uiService.CreateRibbonPanel(uiControlledApplication,bimdataTab, 
            "BIDTP_PN1", 
            "BIDTP панель");
        
        var uploadModelButton = uiService
            .CreatePushButton<RunAboutProgramExternalCommand>
            (
                "Запустить UI",
                null,
                null,
                "Запускает межпроцессорное взаимодействие"
            );
            
        uploadPanel.AddItem(uploadModelButton);

        return bimdataTab;
    } 
}