using System;
using System.IO;
using Serilog;

namespace Example.Server.Revit.Utils
{
    /// <summary>
    /// Конфигуратор для логгера.
    /// </summary>
    public static class LoggerConfigurationUtil
    {
        /// <summary>
        /// Инициализирует логгер с конфигурацией для внешнего приложения.
        /// </summary>
        /// <typeparam name="T">Тип внешнего приложения.</typeparam>
        /// <remarks>Логгер настраивается для записи логов в файл.</remarks>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// Вызывается, когда директория для записи логов не существует и не может быть создана.
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


