using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Tools;
using Xx.Cli.Dotnet.Projects;

namespace Xx.Cli.Dotnet.Commands.New;

public class ExeCommand : Command<ExeCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "exe";
    public static string Description => "Create new exe project.";
    public ILogger Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public ExeCommand(ITemplateWriter templateWriter, ILogger logger)
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(ExeCommandConfiguration cfg, DiscoverConfiguration discoverCfg, CancellationToken ct)
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Debug<string, string>("Create executable project {name} at {location}", name, location);

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
