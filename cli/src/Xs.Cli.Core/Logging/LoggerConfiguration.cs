namespace Xs.Cli.Core.Logging
{
    internal class LoggerConfiguration
    {
        public LogLevel LogLevel { get; set; }

        public bool PrintTime { get; set; } = false;

        public bool PrintThread { get; set; } = false;
    }

    internal class RawLoggerConfiguration
    {
        public bool Trace { get; set; } = false;

        public bool Debug { get; set; } = false;

        public bool Info { get; set; } = false;

        public bool Warn { get; set; } = false;

        public bool Error { get; set; } = false;

        public bool PrintTime { get; set; } = false;

        public bool PrintThread { get; set; } = false;

        public static explicit operator LoggerConfiguration(RawLoggerConfiguration raw)
        {
            var cfg = new LoggerConfiguration();

            cfg.LogLevel = getLevel();
            cfg.PrintTime = raw.PrintTime;
            cfg.PrintThread = raw.PrintThread;

            return cfg;

            LogLevel getLevel()
            {
                if (raw.Trace)
                    return LogLevel.Trace;
                if (raw.Debug)
                    return LogLevel.Debug;
                if (raw.Warn)
                    return LogLevel.Warn;
                if (raw.Error)
                    return LogLevel.Error;
                return LogLevel.Info;
            }
        }
    }
}