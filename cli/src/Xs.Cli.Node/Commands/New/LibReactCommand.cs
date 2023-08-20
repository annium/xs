using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Projects;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node.Commands.New;

public class LibReactCommand : AsyncCommand<LibReactCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject<LibReactCommand>
{
    public static string Id => "lib.react";
    public static string Description => "Create new library project, using React.";
    public ILogger<LibReactCommand> Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public LibReactCommand(
        ITemplateWriter templateWriter,
        ILogger<LibReactCommand> logger
    )
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override async Task HandleAsync(
        LibReactCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Log().Debug($"Create library project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.LibReact");
        _templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class LibReactCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}