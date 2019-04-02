using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class TestProject : LibraryProject, ITestableProject
    {
        public TestProject(SpecialProjectContext context) : base(context) { }

        public Task TestAsync(Env env, CancellationToken token) =>
            RunAsync("test", $"yarn run test", token);
    }
}