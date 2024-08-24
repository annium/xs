using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Tools;
using Xx.Cli.Node.Projects;
using Xx.Cli.Node.Tools;

namespace Xx.Cli.Node.Commands.New;

public class LibCommand : Command<LibCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "lib";
    public static string Description => "Create new library project.";
    public ILogger Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public LibCommand(ITemplateWriter templateWriter, ILogger logger)
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(LibCommandConfiguration cfg, DiscoverConfiguration discoverCfg, CancellationToken ct)
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Debug($"Create library project {name} at {location}");

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
