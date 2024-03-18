using System;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Example.Server.Revit.ExternalCommands;
using Example.Server.Revit.Utils;

namespace Example.Server.Revit.Configurator;

/// <summary>
/// Конфигуратор UI компонентов Revit. Инкапсулирует создание табов, панелей и кнопок приложения
/// </summary>
public class RevitUiConfigurator
{
    private static readonly Lazy<RevitUiConfigurator> Instance = 
        new Lazy<RevitUiConfigurator>(() => new RevitUiConfigurator());

    private RevitUiConfigurator()
    {
    }

    /// <summary>
    ///  Возвращает экземпляр синглтона
    /// </summary>
    /// <returns> Экземпляр синглтона</returns>
    public static RevitUiConfigurator GetInstance () => Instance.Value;
    
    /// <summary>
    /// Конфигурирует Revit UI компоненты
    /// </summary>
    /// <param name="uiControlledApplication"></param>
    /// <returns></returns>
    public RibbonTab ConfigureRevitUiComponents(UIControlledApplication uiControlledApplication)
    {
        var uiService = UiComponentCreateUtil.GetInstance(); 
            
        var bimdataTab = uiService.CreateRibbonTab( uiControlledApplication,"PIDERS");
        
        var uploadPanel = uiService.CreateRibbonPanel(uiControlledApplication,bimdataTab, 
            "PIDERS_PN1", 
            "PIDERS панель");
        
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