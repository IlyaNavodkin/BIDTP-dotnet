using System;
using Autodesk.Revit.UI;
using Example.Server.Revit.Configurator;
using Example.Server.Revit.Extensions;
using Example.Server.Revit.Utils;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Toolkit.External.Handlers;
using Serilog;
using Exception = System.Exception;

namespace Example.Server.Revit;

/// <summary>
/// Основной класс приложение, реализующий запуск и конфигурацию плагина
/// </summary>
/// <remarks>Запуск производится с помощью манифест файла *.addin.</remarks>
public class SimpleDimpleExternalApplication: ExternalApplication
{
    /// <summary>
    ///  The async event handler
    /// </summary>
    public static AsyncEventHandler AsyncEventHandler { get; set; }
    
    /// <inheritdoc/>
    public override void OnStartup()
    {
        AppDomain.CurrentDomain.UnhandledException += this.HandleUnhandledException;

        try
        {
            LoggerConfigurationUtil.InitializeLogger<SimpleDimpleExternalApplication>();
            
            AsyncEventHandler = new AsyncEventHandler();
            
            var configurator = RevitUiConfigurator.GetInstance();
            configurator.ConfigureRevitUiComponents(Application);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Application startup failed");

            Result = Result.Failed;
        }
    }
}