using Annium.Logging.Abstractions;

namespace Xs.Cli.Core.Logging;

public class LoggerConfiguration
{
    public bool Trace { get; set; } = false;
    public bool Debug { get; set; } = false;
    public bool Info { get; set; } = false;

    public static explicit operator LogLevel(LoggerConfiguration raw)
    {
        if (raw.Trace)
            return LogLevel.Trace;

        if (raw.Debug)
            return LogLevel.Debug;

        return LogLevel.Info;
    }
}