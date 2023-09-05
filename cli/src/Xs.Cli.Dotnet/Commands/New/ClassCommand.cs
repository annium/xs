using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Dotnet.Commands.New;

public class ClassCommand : AsyncCommand<ClassCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    private const string ClassTemplate = "Class.cs_tpl";

    public static string Id => "class";
    public static string Description => "Create new classes.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ITemplateWriter _templateWriter;

    public ClassCommand(
        DiscoverProjectsTask discoverTask,
        ITemplateWriter templateWriter,
        ILogger logger
    )
    {
        _discoverTask = discoverTask;
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override async Task HandleAsync(
        ClassCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var output = Path.GetFullPath(Path.Combine(discoverCfg.Root, cfg.Output));
        var projects = await _discoverTask.RunAsync(discoverCfg);
        var project = projects.FirstOrDefault(p => output.StartsWith(p.Directory));

        if (project is null)
        {
            Console.Write("Can't determine project, class will belong to");
            return;
        }

        var names = new List<string>();
        while (true)
        {
            Console.Write("Class name?: ");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
                break;

            names.Add(name);
        }

        if (names.Count == 0)
            return;

        this.Debug($"{names.Count} class(es) to create");

        Directory.CreateDirectory(output);

        foreach (var name in names)
        {
            this.Debug($"Create class {name} at {output}");

            _templateWriter.LoadResources($"{Group.TemplatesDir}.Class");
            _templateWriter.SetRoot(output);

            // setup data
            var ns = $"{project.Name}.{Path.GetRelativePath(project.Directory, output).Replace(Path.DirectorySeparatorChar, '.')}";
            var data = new { ns, name };

            // write files
            _templateWriter.Write(ClassTemplate, $"{name}.cs", data);
            _templateWriter.EnsureAllWritten();
        }
    }
}

public class ClassCommandConfiguration
{
    [Option("o", isRequired: true)]
    [Help("Output directory.")]
    public string Output { get; set; } = string.Empty;
}