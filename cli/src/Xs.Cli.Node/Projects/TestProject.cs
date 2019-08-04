using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class TestProject : SpecialProject<TestProject>, ITestableProject
    {
        public TestProject(SpecialProjectContext<TestProject> context) : base(context) { }

        public Task TestAsync(Env env, string filter, CancellationToken token) =>
            string.IsNullOrWhiteSpace(filter) ?
            RunAsync("test", $"yarn run test", token) :
            RunAsync("test", $"yarn run test --testNamePattern {filter}", token);
    }
}