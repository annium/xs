using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Xs.Cli.Core.Logging
{
    internal class Logger : ILogger
    {
        private static readonly object consoleLock = new object();

        private readonly Func<DateTime> getTime;

        private readonly LoggerConfiguration configuration;

        private readonly IReadOnlyDictionary<LogLevel, ConsoleColor> levelColors;

        public Logger(
            Func<DateTime> getTime,
            LoggerConfiguration configuration
        )
        {
            this.configuration = configuration;
            this.getTime = getTime;

            var levelColors = new Dictionary<LogLevel, ConsoleColor>();
            levelColors[LogLevel.Trace] = ConsoleColor.DarkCyan;
            levelColors[LogLevel.Debug] = ConsoleColor.DarkGray;
            levelColors[LogLevel.Info] = ConsoleColor.White;
            levelColors[LogLevel.Warn] = ConsoleColor.Yellow;
            levelColors[LogLevel.Error] = ConsoleColor.Red;
            this.levelColors = levelColors;
        }

        public void LogTrace(string message) => Log(LogLevel.Trace, message);

        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        public void LogInfo(string message) => Log(LogLevel.Info, message);

        public void LogWarn(string message) => Log(LogLevel.Warn, message);

        public void LogError(Exception exception) => Log(LogLevel.Error, exception);

        private void Log(LogLevel level, string message)
        {
            if (level < configuration.LogLevel)
                return;

            lock(consoleLock)
            {
                var currentColor = Console.ForegroundColor;
                Console.ForegroundColor = levelColors[level];
                WriteLine(level, message);
                Console.ForegroundColor = currentColor;
            }
        }

        private void Log(LogLevel level, Exception exception)
        {
            if (level < configuration.LogLevel)
                return;

            lock(consoleLock)
            {
                var currentColor = Console.ForegroundColor;
                Console.ForegroundColor = levelColors[level];

                if (exception is AggregateException aggregateException)
                {
                    var errors = aggregateException.Flatten().InnerExceptions;
                    WriteLine(level, $"Errors ({errors.Count}):");

                    foreach (var error in errors)
                        WriteLine(level, getExceptionMessage(error));
                }
                else
                    WriteLine(level, getExceptionMessage(exception));

                Console.ForegroundColor = currentColor;
            }

            string getExceptionMessage(Exception e) => configuration.LogLevel > LogLevel.Debug ?
                exception.Message :
                $"{exception.Message}{Environment.NewLine}{exception.StackTrace}";
        }

        private void WriteLine(LogLevel logLevel, string message)
        {
            var builder = new StringBuilder();

            if (configuration.PrintTime)
                builder.Append($"[{getTime().ToString("HH:mm:ss.fff")}] ");

            if (configuration.PrintThread)
                builder.Append($"[{Thread.CurrentThread.ManagedThreadId,3}] ");

            builder.Append(message);

            Console.WriteLine(builder.ToString());
        }
    }
}