using System.Threading;
using System.Threading.Tasks;

namespace Xs.Cli.Core.Projects
{
    public interface IInstallableProject : IProject
    {
        Task InstallAsync(bool force, CancellationToken token);
    }
}