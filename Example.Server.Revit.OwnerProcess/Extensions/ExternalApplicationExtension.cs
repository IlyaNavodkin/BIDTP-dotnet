using System;
using Nice3point.Revit.Toolkit.External;
using Serilog;

namespace Example.Server.Revit.Extensions;

/// <summary>
///  Расширения для <see cref="ExternalApplication"/>
/// </summary>
public static class  ExternalApplicationExtension
{
    /// <summary>
    ///  Перехватывает и обрабатывает необработанные исключения 
    /// </summary>
    /// <param name="externalApplication"> <see cref="ExternalApplication"/></param>
    /// <param name="sender"> Сендер</param>
    /// <param name="e"> Аргументы события</param>
    public static void HandleUnhandledException(this ExternalApplication externalApplication, 
        object sender, UnhandledExceptionEventArgs e)
    {
        if( e.ExceptionObject is not Exception exception ) return;
            
        Log.Fatal(exception, "Unhandled exception");
    }
}