using System.Threading;
using System.Threading.Tasks;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Projects;

public interface ITestableProject : IProject
{
    Task TestAsync(Env env, string filter, CancellationToken ct);
}
