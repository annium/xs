using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Logging;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Projects;
using Xx.Cli.Core.Tasks;
using Xx.Cli.Dotnet.Projects;

namespace Xx.Cli.Dotnet.Commands.Sln;

public class SyncCommand : AsyncCommand<DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    private const string SlnExtension = ".sln";

    public static string Id => "sync";
    public static string Description => "Create sln file from project.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IShell _shell;

    public SyncCommand(DiscoverProjectsTask discoverTask, IShell shell, ILogger logger)
    {
        _discoverTask = discoverTask;
        _shell = shell;
        Logger = logger;
    }

    public override async Task HandleAsync(DiscoverConfiguration discoverCfg, CancellationToken ct)
    {
        var root = Directory.GetCurrentDirectory();
        var projects = (await _discoverTask.RunAsync(discoverCfg)).OfType<IPlatformProject>().ToArray();

        await Task.WhenAll(
            projects
                .SelectMany(x => x.Solutions)
                .Distinct()
                .Select(async name =>
                {
                    var solutionProjects = projects.Where(x => x.Solutions.Contains(name)).ToArray();
                    await SyncSolutionAsync(root, name, solutionProjects);
                })
        );
    }

    private async Task SyncSolutionAsync(string root, string name, IReadOnlyCollection<IPlatformProject> projects)
    {
        var slnFile = SlnFile(root, name);
        this.Debug<string>("Write solution file {slnFile}", slnFile);
        await _shell.Cmd($"dotnet new sln --name {name} --output {root} --force").RunAsync();

        var currentProjects = await GetSolutionProjectPathsAsync(root, name);
        var removedProjects = currentProjects.Where(path => projects.All(pp => pp.File != path)).ToList();

        // add current projects
        foreach (var project in projects)
        {
            var parent =
                Directory.GetParent(project.Directory)?.FullName
                ?? throw new DirectoryNotFoundException($"Directory {project.Directory} has no parent directory");
            if (parent == root)
            {
                this.Debug("Add {project} to solution file at root", project);
                await _shell.Cmd($"dotnet sln {slnFile} add {project.File}").RunAsync();
            }
            else
            {
                var folder = Path.GetRelativePath(root, parent);
                this.Debug<IProject, string>("Add {project} to solution file at {folder}", project, folder);
                await _shell.Cmd($"dotnet sln {slnFile} add --solution-folder {folder} {project.File}").RunAsync();
            }
        }

        // delete missing projects
        foreach (var path in removedProjects)
        {
            this.Debug<string>("Remove {path} from solution file", path);
            await _shell.Cmd($"dotnet sln {slnFile} remove {path}").RunAsync();
        }
    }

    private async Task<IEnumerable<string>> GetSolutionProjectPathsAsync(string root, string name)
    {
        var slnFile = SlnFile(root, name);

        var result = await _shell.Cmd($"dotnet sln {slnFile} list").RunAsync();
        if (!result.IsSuccess)
            return [];

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
