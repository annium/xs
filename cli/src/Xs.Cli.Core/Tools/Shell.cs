using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;

namespace Xs.Cli.Core.Tools
{
    internal class Shell : IShell
    {
        private readonly ILogger logger;

        public Shell(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public Task<ShellResult> RunAsync(string command, CancellationToken token = default(CancellationToken)) =>
            RunAsync(null, command, token);

        public Task<ShellResult> RunAsync(ProcessStartInfo startInfo, string command, CancellationToken token = default(CancellationToken))
        {
            var process = GetProcess(startInfo, command);

            return StartProcess(process, token).Task;
        }

        public ShellAsyncResult Start(string command, CancellationToken token = default(CancellationToken)) =>
            Start(null, command, token);

        public ShellAsyncResult Start(ProcessStartInfo startInfo, string command, CancellationToken token = default(CancellationToken))
        {
            var process = GetProcess(startInfo, command);

            var task = StartProcess(process, token).Task;

            return new ShellAsyncResult(
                process.StandardInput,
                process.StandardOutput,
                process.StandardError,
                task
            );
        }

        private Process GetProcess(ProcessStartInfo startInfo, string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new InvalidOperationException("Shell command must be non-empty.");

            var process = new Process();

            process.EnableRaisingEvents = true;

            if (startInfo != null)
                process.StartInfo = startInfo;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            var args = command.Split(' ');
            process.StartInfo.FileName = args[0];
            process.StartInfo.Arguments = string.Join(" ", args.Skip(1));

            logger.LogTrace($"shell: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

            return process;
        }

        private TaskCompletionSource<ShellResult> StartProcess(Process process, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<ShellResult>();

            process.Start();

            // track token cancellation and kill process if requested
            // as far as there's no way to know if process was killed or finished on it's own - track it manually
            var killed = false;
            var registration = token.Register(() =>
            {
                killed = true;
                process.Kill();
            });

            // this will be called when process finished on it's own, or is killed
            process.Exited += (sender, e) =>
            {
                registration.Dispose();
                if (killed)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(GetResult(process));
                process.Dispose();
            };

            return tcs;
        }

        private ShellResult GetResult(Process process)
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            return new ShellResult(process.ExitCode, output, error);
        }
    }
}