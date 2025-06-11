using System.IO;
using System.Threading;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Dotnet.Projects;

namespace Annium.Xs.Cli.Dotnet.Commands.New;

public class WebCommand : Command<WebCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "web";
    public static string Description => "Create new web project.";
    public ILogger Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public WebCommand(ITemplateWriter templateWriter, ILogger logger)
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(WebCommandConfiguration cfg, DiscoverConfiguration discoverCfg, CancellationToken ct)
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Debug<string, string>("Create web project {name} at {location}", name, location);

        _templateWriter.LoadResources($"{Group.TemplatesDir}.Web");
        _templateWriter.SetRoot(Path.Combine(location, name));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class WebCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}
