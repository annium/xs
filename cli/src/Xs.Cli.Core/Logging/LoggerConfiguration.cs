using Annium.Logging.Abstractions;

namespace Xs.Cli.Core.Logging
{
    internal class LoggerConfiguration
    {
        public bool Trace { get; set; } = false;

        public bool Debug { get; set; } = false;

        public bool Info { get; set; } = false;

        public static explicit operator Annium.Logging.Abstractions.LoggerConfiguration(LoggerConfiguration raw)
        {
            var logLevel = getLevel();

            return new Annium.Logging.Abstractions.LoggerConfiguration(logLevel);

            LogLevel getLevel()
            {
                if (raw.Trace)
                    return LogLevel.Trace;
                if (raw.Debug)
                    return LogLevel.Debug;
                return LogLevel.Info;
            }
        }
    }
}