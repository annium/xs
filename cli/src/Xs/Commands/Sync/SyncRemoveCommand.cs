using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Xs.Commands.Sync;

internal class SyncRemoveCommand : AsyncCommand<SyncRemoveCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "remove";
    public static string Description => "Remove repository from sync config";
    private readonly SyncConfigurator _configurator;

    public SyncRemoveCommand(SyncConfigurator configurator)
    {
        _configurator = configurator;
    }

    public override Task HandleAsync(SyncRemoveCommandConfiguration cfg, CancellationToken ct)
    {
        var projects = _configurator.Read();

        var path = Path.GetFullPath(cfg.Path.TrimEnd('/'));
        var project = projects.SingleOrDefault(x => x.Path == path);

        if (project is not null)
            projects.Remove(project);

        _configurator.Write(projects);

        return Task.CompletedTask;
    }
}

internal class SyncRemoveCommandConfiguration
{
    [Position(1)]
    [Help("Repository path.")]
    public string Path { get; set; } = string.Empty;
}
