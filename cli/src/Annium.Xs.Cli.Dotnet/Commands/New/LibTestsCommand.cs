using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Dotnet.Projects;

namespace Annium.Xs.Cli.Dotnet.Commands.New;

public class LibTestsCommand
    : Command<LibTestsCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "lib.tests";
    public static string Description => "Create new library tests project.";
    public ILogger Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public LibTestsCommand(ITemplateWriter templateWriter, ILogger logger)
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

        this.Debug<string, string>("Create library tests project {name} at {location}", name, location);

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
