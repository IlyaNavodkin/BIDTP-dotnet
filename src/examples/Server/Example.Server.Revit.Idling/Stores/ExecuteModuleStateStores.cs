using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Example.Server.Revit.Stores;

public class ExecuteModuleStateStores
{
    private readonly List<ExecuteModuleState> _states = new();

    public ExecuteModuleState StartModule(string name, string relativePath, string pipeName)
    {
        var state = new ExecuteModuleState(name, relativePath, pipeName);

        state.Process.EnableRaisingEvents = true;
        state.Process.Exited += Process_Exited;

        _states.Add(state);
        return state;
    }

    private void Process_Exited(object sender, EventArgs e)
    {
        if (sender is Process process)
        {
            var state = _states.FirstOrDefault(s => s.Process == process);
            if (state != null)
            {
                _states.Remove(state);
            }
        }
    }

    public bool IsRunning(string name)
    {
        return _states.Any(s => s.Name == name);
    }
}
