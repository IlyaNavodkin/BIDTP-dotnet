using Example.Server.Revit.Constants;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Example.Server.Revit.Stores;

public class ExecuteModuleState
{
    public ExecuteModuleState(string moduleName, string moduleRelativePath, string pipeName, string extraArguments = "")
    {
        Name = moduleName;
        RelativeExecutePath = moduleRelativePath;

        Process = RunClientProcess(moduleName, moduleRelativePath, pipeName, extraArguments);
    }

    public string RelativeExecutePath { get; }
    public string Name { get; }
    public Process Process { get; }

    private Process RunClientProcess(string name, string relativePath, string pipeName, string extraArguments)
    {
        var executionPath = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(executionPath);
        var fullPath = $"{directory}{relativePath}";

        var processId = Process.GetCurrentProcess().Id;
        var arguments = $"{ServerApplicationConstants.ModuleRunArgument.PipeName}=\"{pipeName}\" " +
                        $"{ServerApplicationConstants.ModuleRunArgument.Pid}=\"{processId}\" " +
                        $"{extraArguments}".Trim(); 

        var childProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fullPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        if (!childProcess.Start())
            throw new InvalidOperationException("Failed to start the process.");

        return childProcess;
    }
}
