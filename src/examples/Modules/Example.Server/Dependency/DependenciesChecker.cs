using System.Diagnostics;
using System.Linq;

namespace BIDTP.Dotnet.Core.Dependency;

/// <summary>
///  Check if dotnet is installed in windows machine
/// </summary>
public class DependenciesChecker
{
    /// <summary>
    ///  Check if dotnet is installed in windows machine from dotnet process
    /// </summary>
    /// <param name="dotnetRuntimeVersion"> The version of the dotnet runtime </param>
    /// <returns> True if dotnet is installed </returns>
    public static bool CheckDotnetInstallation(string dotnetRuntimeVersion)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "--list-runtimes",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            var process = Process.Start(startInfo)!;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var splitOutput = output.Split('\n');
            var isDotnetInstalled = splitOutput
                .Where(line => line.Contains("Microsoft.WindowsDesktop.App"))
                .Any(line => line.Contains($"{dotnetRuntimeVersion}."));
            
            return isDotnetInstalled;
        }
        catch
        {
            return false;
        }
    }
}