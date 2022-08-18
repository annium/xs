using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New;

public class LibTestsCommand : Command<LibTestsCommandConfiguration, DiscoverConfiguration>, ILogSubject<LibTestsCommand>
{
    public override string Id => "lib.tests";
    public override string Description => "Create new library tests project.";
    public ILogger<LibTestsCommand> Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public LibTestsCommand(
        ITemplateWriter templateWriter,
        ILogger<LibTestsCommand> logger
    )
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(
        LibTestsCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Log().Debug($"Create library tests project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.LibTests");
        _templateWriter.SetRoot(Path.Combine(location, name));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class LibTestsCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}