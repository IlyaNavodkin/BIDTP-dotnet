using System;
using Nice3point.Revit.Toolkit.External;
using Serilog;

namespace Example.Server.Revit.ExternalCommand.Extensions;

/// <summary>
///  Extension for <see cref="ExternalApplication"/>
/// </summary>
public static class  ExternalApplicationExtension
{
    /// <summary>
    ///  Handle unhandled exception in the external application
    /// </summary>
    /// <param name="externalApplication"> <see cref="ExternalApplication"/></param>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event args</param>
    public static void HandleUnhandledException(this ExternalApplication externalApplication, 
        object sender, UnhandledExceptionEventArgs e)
    {
        if( e.ExceptionObject is not Exception exception ) return;
            
        Log.Fatal(exception, "Unhandled exception");
    }
}