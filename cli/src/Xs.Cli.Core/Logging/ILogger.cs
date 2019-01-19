using System;

namespace Xs.Cli.Core.Logging
{
    public interface ILogger
    {
        void LogTrace(string message);

        void LogDebug(string message);

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(Exception exception);
    }
}