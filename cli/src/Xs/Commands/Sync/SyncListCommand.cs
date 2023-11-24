using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Xs.Commands.Sync;

internal class SyncListCommand : AsyncCommand<SyncListCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "list";
    public static string Description => "List repositories for sync";
    private readonly SyncConfigurator _configurator;

    public SyncListCommand(SyncConfigurator configurator)
    {
        _configurator = configurator;
    }

    public override Task HandleAsync(SyncListCommandConfiguration cfg, CancellationToken ct)
    {
        var projects = _configurator.Read();
        if (!string.IsNullOrWhiteSpace(cfg.Group))
            projects = projects
                .Where(x => string.Equals(x.Group, cfg.Group, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

        foreach (var project in projects)
            Console.WriteLine(project.ToString());

        return Task.CompletedTask;
    }
}

internal class SyncListCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Project group.")]
    public string Group { get; set; } = string.Empty;
}
