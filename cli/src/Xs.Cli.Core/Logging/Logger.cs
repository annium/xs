using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace Xs.Cli.Core.Logging
{
    internal class Logger : ILogger
    {
        private static readonly DateTimeZone tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

        private static readonly object consoleLock = new object();

        private readonly Func<Instant> getInstant;

        private readonly LoggerConfiguration configuration;

        private readonly IReadOnlyDictionary<LogLevel, ConsoleColor> levelColors;

        public Logger(
            Func<Instant> getInstant,
            LoggerConfiguration configuration
        )
        {
            this.configuration = configuration;
            this.getInstant = getInstant;

            var levelColors = new Dictionary<LogLevel, ConsoleColor>();
            levelColors[LogLevel.Trace] = ConsoleColor.DarkGray;
            levelColors[LogLevel.Debug] = ConsoleColor.Gray;
            levelColors[LogLevel.Info] = ConsoleColor.White;
            levelColors[LogLevel.Warn] = ConsoleColor.Yellow;
            levelColors[LogLevel.Error] = ConsoleColor.Red;
            this.levelColors = levelColors;
        }

        public void Trace(string message) => Log(LogLevel.Trace, message);

        public void Debug(string message) => Log(LogLevel.Debug, message);

        public void Info(string message) => Log(LogLevel.Info, message);

        public void Warn(string message) => Log(LogLevel.Warn, message);

        public void Error(Exception exception) => Log(LogLevel.Error, exception);

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

            builder.Append($"[{getInstant().InZone(tz).LocalDateTime.ToString("HH:mm:ss.fff", null)}] ");

            builder.Append(message);

            Console.WriteLine(builder.ToString());
        }
    }
}