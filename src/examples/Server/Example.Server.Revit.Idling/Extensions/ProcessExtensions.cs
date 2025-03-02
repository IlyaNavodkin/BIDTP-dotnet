using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Example.Server.Revit.Extensions;

internal static class ProcessExtensions
{
    public static void WaitForProcessExit(this Process process)
    {
        var frame = new DispatcherFrame();

        process.Exited += (sender, args) => frame.Continue = false;

        if (!process.HasExited)
            Dispatcher.PushFrame(frame);
    }
}
