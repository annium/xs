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

public class LibCommand : AsyncCommand<LibCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject<LibCommand>
{
    public static string Id => "lib";
    public static string Description => "Create new library project.";
    public ILogger<LibCommand> Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public LibCommand(
        ITemplateWriter templateWriter,
        ILogger<LibCommand> logger
    )
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override async Task HandleAsync(
        LibCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Log().Debug($"Create library project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.Lib");
        _templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class LibCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}