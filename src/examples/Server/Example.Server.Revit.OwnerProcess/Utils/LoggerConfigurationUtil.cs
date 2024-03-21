using System;
using System.IO;
using Serilog;

namespace Example.Server.Revit.OwnerProcess.Utils
{
    /// <summary>
    ///  The logger configuration util 
    /// </summary>
    public static class LoggerConfigurationUtil
    {
        /// <summary>
        ///  Initializes the logger. 
        /// </summary>
        /// <typeparam name="T"> The type of the logger .</typeparam>
        /// <remarks>The logger should be placed in *.addin .</remarks>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///  Thrown if the directory is not found. 
        /// </exception>
        public static void InitializeLogger<T>() where T : class
        {
            var assemblyPath = Path.GetDirectoryName(typeof(T).Assembly.Location);
            var logsPath = $"{assemblyPath}\\Logs";
            var pluginName = typeof(T).Name;
            var logName = $"{pluginName}.log";
            var logPath = Path.Combine(logsPath, logName);

            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(logPath,
                    retainedFileCountLimit: 3,
                    rollingInterval: RollingInterval.Hour,
                    rollOnFileSizeLimit: true)
                .Enrich.WithProperty("Application", pluginName)
                .CreateLogger();

            Log.Information($"{pluginName} log running...\n");
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
        }
    }
}


