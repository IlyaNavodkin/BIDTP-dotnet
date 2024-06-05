using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Iteraction.Logger
{
    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null; 
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, 
                Exception, string> formatter)
        {
            ConsoleColor color;

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    color = ConsoleColor.Gray;
                    break;
                case LogLevel.Information:
                    color = ConsoleColor.Green;
                    break;
                case LogLevel.Warning:
                    color = ConsoleColor.Yellow; 
                    break;
                case LogLevel.Error:
                    color = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    color = ConsoleColor.Magenta; 
                    break;
                default:
                    color = ConsoleColor.White; 
                    break;
            }

            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Console.WriteLine($"{DateTime.Now:o} [{logLevel}] - {formatter(state, exception)}");

            Console.ForegroundColor = previousColor;
        }
    }
}
