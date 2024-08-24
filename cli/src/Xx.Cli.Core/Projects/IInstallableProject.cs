using System.Threading;
using System.Threading.Tasks;

namespace Xx.Cli.Core.Projects;

public interface IInstallableProject : IProject
{
    Task InstallAsync(bool force, CancellationToken ct);
}
