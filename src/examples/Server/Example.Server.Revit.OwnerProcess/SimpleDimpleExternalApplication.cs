using System;
using Autodesk.Revit.UI;
using Example.Server.Revit.OwnerProcess.Configurator;
using Example.Server.Revit.OwnerProcess.Extensions;
using Example.Server.Revit.OwnerProcess.Utils;
using Nice3point.Revit.Toolkit.External;
using Nice3point.Revit.Toolkit.External.Handlers;
using Serilog;
using Exception = System.Exception;

namespace Example.Server.Revit.OwnerProcess;

/// <summary>
///  The simple dimple external application
/// </summary>
/// <remarks>The application should be placed in *.addin.</remarks>
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