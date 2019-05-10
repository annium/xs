using System.Threading.Tasks;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IDependencyManager
    {
        ProjectType Type { get; }

        Task<Package[]> GetVersionsAsync(Package package, Configuration configuration);
    }
}