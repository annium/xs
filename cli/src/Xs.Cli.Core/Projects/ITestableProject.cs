using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public interface ITestableProject : IProject
{
    Task TestAsync(Env env, string filter, CancellationToken ct);
}
