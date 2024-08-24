using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Tools;
using Xx.Cli.Node.Projects;
using Xx.Cli.Node.Tools;

namespace Xx.Cli.Node.Commands.New;

public class AppReactCommand
    : Command<AppReactCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "app.react";
    public static string Description => "Create new app project, using React.";
    public ILogger Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public AppReactCommand(ITemplateWriter templateWriter, ILogger logger)
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(
        AppReactCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Debug($"Create app project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.AppReact");
        _templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class AppReactCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}
