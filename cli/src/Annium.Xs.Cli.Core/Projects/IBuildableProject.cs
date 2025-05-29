using System.Threading;
using System.Threading.Tasks;
using Annium.Xs.Cli.Core.Models;

namespace Annium.Xs.Cli.Core.Projects;

public interface IBuildableProject : IProject
{
    Task BuildAsync(Env env, bool force, CancellationToken ct);
}
