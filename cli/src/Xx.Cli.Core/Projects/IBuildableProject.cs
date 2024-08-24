using System.Threading;
using System.Threading.Tasks;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Projects;

public interface IBuildableProject : IProject
{
    Task BuildAsync(Env env, bool force, CancellationToken ct);
}
