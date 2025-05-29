using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Commands.Sync;

internal class SyncSetCommand : AsyncCommand<SyncAddCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "set";
    public static string Description => "Setup repository in sync config";
    private readonly SyncConfigurator _configurator;

    public SyncSetCommand(SyncConfigurator configurator)
    {
        _configurator = configurator;
    }

    public override Task HandleAsync(SyncAddCommandConfiguration cfg, CancellationToken ct)
    {
        var projects = _configurator.Read();

        var path = Path.GetFullPath(cfg.Path.TrimEnd('/'));
        var project = projects.SingleOrDefault(x => x.Path == path);

        if (project is not null)
            projects.Remove(project);

        projects.Add(
            new SyncProject
            {
                Path = path,
                Group = cfg.Group,
                Config = new SyncProjectConfig { Push = cfg.Push },
            }
        );

        _configurator.Write(projects);

        return Task.CompletedTask;
    }
}

internal class SyncAddCommandConfiguration
{
    [Position(1)]
    [Help("Project repository path.")]
    public string Path { get; set; } = string.Empty;

    [Position(2)]
    [Help("Project group.")]
    public string Group { get; set; } = string.Empty;

    [Option]
    [Help("Push local branches.")]
    public bool Push { get; set; }
}
