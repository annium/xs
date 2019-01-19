using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IBuildableProject : IProject
    {
        Task BuildAsync(Env env, CancellationToken token);
    }
}