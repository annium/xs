using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects;

internal class TestProject : PlatformProject, ITestableProject
{
    public TestProject(PlatformProjectContext context)
        : base(context) { }

    public Task TestAsync(Env env, string filter, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(filter)
            ? RunAsync("test", $"pnpm test", ct)
            : RunAsync("test", $"pnpm test --testNamePattern {filter}", ct);
}
