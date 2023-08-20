using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New;

public class ExeCommand : AsyncCommand<ExeCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject<ExeCommand>
{
    public static string Id => "exe";
    public static string Description => "Create new exe project.";
    public ILogger<ExeCommand> Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public ExeCommand(
        ITemplateWriter templateWriter,
        ILogger<ExeCommand> logger
    )
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override async Task HandleAsync(
        ExeCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Log().Debug($"Create executable project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.Exe");
        _templateWriter.SetRoot(Path.Combine(location, name));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class ExeCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}