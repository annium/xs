using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New;

public class WebAssemblyLibCommand : Command<WebAssemblyLibCommandConfiguration, DiscoverConfiguration>, ILogSubject<WebAssemblyLibCommand>
{
    public override string Id => "wasm.lib";
    public override string Description => "Create new WebAssembly Library project.";
    public ILogger<WebAssemblyLibCommand> Logger { get; }
    private readonly ITemplateWriter _templateWriter;

    public WebAssemblyLibCommand(
        ITemplateWriter templateWriter,
        ILogger<WebAssemblyLibCommand> logger
    )
    {
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(
        WebAssemblyLibCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = discoverCfg.Root;
        var name = cfg.Name;

        this.Log().Debug($"Create WebAssembly Library project {name} at {location}");

        _templateWriter.LoadResources($"{Group.TemplatesDir}.WebAssemblyLibrary");
        _templateWriter.SetRoot(Path.Combine(location, name));

        // setup data
        var data = new { name };

        // write files
        _templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
        _templateWriter.WriteAll(data);
        _templateWriter.EnsureAllWritten();
    }
}

public class WebAssemblyLibCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Name { get; set; } = string.Empty;
}