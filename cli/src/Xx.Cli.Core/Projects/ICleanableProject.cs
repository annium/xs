using System.Threading;
using System.Threading.Tasks;

namespace Xx.Cli.Core.Projects;

public interface ICleanableProject : IProject
{
    Task CleanAsync(bool force, CancellationToken ct);
}
