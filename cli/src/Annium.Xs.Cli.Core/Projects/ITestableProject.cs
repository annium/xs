using System.Threading;
using System.Threading.Tasks;
using Annium.Xs.Cli.Core.Models;

namespace Annium.Xs.Cli.Core.Projects;

public interface ITestableProject : IProject
{
    Task TestAsync(Env env, string filter, CancellationToken ct);
}
