namespace Xs.Cli.Core.Logging
{
    public class LoggerConfiguration
    {
        public LogLevel LogLevel { get; internal set; }
    }

    internal class RawLoggerConfiguration
    {
        public bool Trace { get; set; } = false;

        public bool Debug { get; set; } = false;

        public bool Info { get; set; } = false;

        public static explicit operator LoggerConfiguration(RawLoggerConfiguration raw)
        {
            var cfg = new LoggerConfiguration();

            cfg.LogLevel = getLevel();

            return cfg;

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