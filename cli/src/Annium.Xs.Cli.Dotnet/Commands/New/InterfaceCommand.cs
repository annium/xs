using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tools;

namespace Annium.Xs.Cli.Dotnet.Commands.New;

public class InterfaceCommand
    : AsyncCommand<InterfaceCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    private const string InterfaceTemplate = "Interface.cs_tpl";

    public static string Id => "interface";
    public static string Description => "Create new interfaces.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ITemplateWriter _templateWriter;

    public InterfaceCommand(DiscoverProjectsTask discoverTask, ITemplateWriter templateWriter, ILogger logger)
    {
        _discoverTask = discoverTask;
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override async Task HandleAsync(
        InterfaceCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var output = Path.GetFullPath(Path.Combine(discoverCfg.Root, cfg.Output));
        var projects = await _discoverTask.RunAsync(discoverCfg);
        var project = projects.FirstOrDefault(p => output.StartsWith(p.Directory));

        if (project is null)
        {
            Console.Write("Can't determine project, interface will belong to");
            return;
        }

        var names = new List<string>();
        while (true)
        {
            Console.Write("Interface name?: ");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
                break;

            names.Add(name);
        }

        if (names.Count == 0)
            return;

        this.Debug("{namesCount} interface(s) to create", names.Count);

        Directory.CreateDirectory(output);

        foreach (var name in names)
        {
            this.Debug<string, string>("Create interface {name} at {output}", name, output);

            _templateWriter.LoadResources($"{Group.TemplatesDir}.Interface");
            _templateWriter.SetRoot(output);

            // setup data
            var ns =
                $"{project.Name}.{Path.GetRelativePath(project.Directory, output).Replace(Path.DirectorySeparatorChar, '.')}";
            var data = new { ns, name };

            // write files
            _templateWriter.Write(InterfaceTemplate, $"{name}.cs", data);
            _templateWriter.EnsureAllWritten();
        }
    }
}

public class InterfaceCommandConfiguration
{
    [Option("o", isRequired: true)]
    [Help("Output directory.")]
    public string Output { get; set; } = string.Empty;
}
