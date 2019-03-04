using System;

namespace Xs.Cli.Core.Logging
{
    public interface ILogger
    {
        void Trace(string message);

        void Debug(string message);

        void Info(string message);

        void Warn(string message);

        void Error(Exception exception);

        void Pipe(LogLevel level, string message);
    }
}