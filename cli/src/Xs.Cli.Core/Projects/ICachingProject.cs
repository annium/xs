using System.Threading;
using System.Threading.Tasks;

namespace Xs.Cli.Core.Projects;

public interface ICachingProject : IProject
{
    Task ClearCacheAsync(CancellationToken ct);
}