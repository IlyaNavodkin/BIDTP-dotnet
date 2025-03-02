using System;
using System.Diagnostics;

namespace Example.Server.Revit.Helpers;

internal static class OsUiHelpers
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    public static void DisableRevitUI()
    {
        var mainWindow = Process.GetCurrentProcess().MainWindowHandle;
        if (mainWindow != IntPtr.Zero)
            EnableWindow(mainWindow, false);
    }

    public static void EnableRevitUI()
    {
        var mainWindow = Process.GetCurrentProcess().MainWindowHandle;
        if (mainWindow != IntPtr.Zero)
            EnableWindow(mainWindow, true);
    }
}
