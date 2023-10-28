using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Logging;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands;

public class SlnCommand : AsyncCommand<SlnCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    private const string SlnExtension = ".sln";

    public static string Id => "sln";
    public static string Description => "Create sln file from project.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IShell _shell;

    public SlnCommand(DiscoverProjectsTask discoverTask, IShell shell, ILogger logger)
    {
        _discoverTask = discoverTask;
        _shell = shell;
        Logger = logger;
    }

    public override async Task HandleAsync(
        SlnCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var root = Directory.GetCurrentDirectory();
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var preservedProjects = allProjects.OfType<ISpecialProject>().ToArray();

        var slnFile = SlnFile(root, cfg.Name);
        this.Debug($"Write solution file {slnFile}");
        await _shell.Cmd($"dotnet new sln --name {cfg.Name} --output {root} --force").RunAsync();

        var currentProjects = await GetSolutionProjectPathsAsync(root, cfg.Name);
        var removedProjects = currentProjects.Where(path => preservedProjects.All(pp => pp.File != path)).ToList();

        // add current projects
        foreach (var project in preservedProjects)
        {
            var parent =
                Directory.GetParent(project.Directory)?.FullName
                ?? throw new DirectoryNotFoundException($"Directory {project.Directory} has no parent directory");
            if (parent == root)
            {
                this.Debug($"Add {project} to solution file at root");
                await _shell.Cmd($"dotnet sln {slnFile} add {project.File}").RunAsync();
            }
            else
            {
                var folder = Path.GetRelativePath(root, parent);
                this.Debug($"Add {project} to solution file at {folder}");
                await _shell.Cmd($"dotnet sln {slnFile} add --solution-folder {folder} {project.File}").RunAsync();
            }
        }

        // delete missing projects
        foreach (var path in removedProjects)
        {
            this.Debug($"Remove {path} from solution file");
            await _shell.Cmd($"dotnet sln {slnFile} remove {path}").RunAsync();
        }
    }

    private async Task<IEnumerable<string>> GetSolutionProjectPathsAsync(string root, string name)
    {
        var slnFile = SlnFile(root, name);

        var result = await _shell.Cmd($"dotnet sln {slnFile} list").RunAsync();
        if (!result.IsSuccess)
            return Enumerable.Empty<string>();

        var output = result.Output.Trim().Split(Environment.NewLine);

        // As of now, dotnet sln list doesn't provide machine-friendly output
        // If there are no projects in sln, output is:
        //
        // If there are any projects in sln, output is:
        //      Project(s)
        //      ----------
        //      path/to/project.csproj
        // So, code belong is targeting that specific behavior
        return output.Length > 2
            ? output.Skip(2).Select(p => Path.Combine(root, p)).ToList()
            : Enumerable.Empty<string>();
    }

    private string SlnFile(string root, string name) => Path.Combine(root, $"{name}{SlnExtension}");
}

public class SlnCommandConfiguration
{
    [Position(1)]
    [Help("Solution file name.")]
    public string Name { get; set; } = string.Empty;

    [Option("d")]
    [Help("Directory to include.")]
    public string[] Directories
    {
        get => _directories;
        set => _directories = value.Select(Path.GetFullPath).ToArray();
    }

    private string[] _directories = { Directory.GetCurrentDirectory() };
}
