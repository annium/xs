using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New;

public class WebAssemblyAppCommand : Command<WebAssemblyAppCommandConfiguration, DiscoverConfiguration>, ILogSubject<WebAssemblyAppCommand>
{
    public override string Id => "wasm.app";
    public override string Description => "Create new WebAssembly Application project.";
    public ILogger<WebAssemblyAppCommand> Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public WebAssemblyAppCommand(
        ITemplateWriter templateWriter,
        ILogger<WebAssemblyAppCommand> logger
    )
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(
        WebAssemblyAppCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Log().Debug($"Create WebAssembly Application project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.WebAssemblyApplication");
        _templateWriter.SetRoot(Path.Combine(location, name));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class WebAssemblyAppCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}