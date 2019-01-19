using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Xs.Cli.Core.Tools
{
    public interface IShell
    {
        Task<ShellResult> RunAsync(string command, CancellationToken token = default(CancellationToken));

        Task<ShellResult> RunAsync(ProcessStartInfo startInfo, string command, CancellationToken token = default(CancellationToken));

        ShellAsyncResult Start(string command, CancellationToken token = default(CancellationToken));

        ShellAsyncResult Start(ProcessStartInfo startInfo, string command, CancellationToken token = default(CancellationToken));
    }
}