using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands
{
    public class SlnCommand : AsyncCommand<SlnCommandConfiguration, DiscoverConfiguration>
    {
        private const string SlnExtension = ".sln";

        public override string Id { get; } = "sln";
        public override string Description { get; } = "Create sln file from project.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly IShell shell;
        private readonly ILogger<SlnCommand> logger;

        public SlnCommand(
            DiscoverProjectsTask discoverTask,
            IShell shell,
            ILogger<SlnCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.shell = shell;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            SlnCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var root = discoverCfg.Root;
            var preservedProjects = discoverTask.Run(discoverCfg)
                .OfType<ISpecialProject>()
                .ToArray();

            var slnFile = SlnFile(root, cfg.Name);
            logger.Debug($"Write solution file {slnFile}");
            await shell.Cmd($"dotnet new sln --name {cfg.Name} --output {root}").RunAsync();

            var currentProjects = await GetSolutsionProjectPathsAsync(root, cfg.Name);
            var removedProjects = currentProjects
                .Where(path => !preservedProjects.Any(pp => pp.File == path))
                .ToList();

            // add current projects
            foreach (var project in preservedProjects)
            {
                var parent = Directory.GetParent(project.Directory).FullName;
                if (parent == root)
                {
                    logger.Debug($"Add {project} to solution file at root");
                    await shell.Cmd($"dotnet sln {slnFile} add {project.File}").RunAsync();
                }
                else
                {
                    var folder = Path.GetRelativePath(root, parent);
                    logger.Debug($"Add {project} to solution file at {folder}");
                    await shell.Cmd($"dotnet sln {slnFile} add --solution-folder {folder} {project.File}").RunAsync();
                }
            }

            // delete missing projects
            foreach (var path in removedProjects)
            {
                logger.Debug($"Remove {path} from solution file");
                await shell.Cmd($"dotnet sln {slnFile} remove {path}").RunAsync();
            }
        }

        private async Task<IEnumerable<string>> GetSolutsionProjectPathsAsync(string root, string name)
        {
            var slnFile = SlnFile(root, name);

            var result = await shell.Cmd($"dotnet sln {slnFile} list").RunAsync();
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
            return output.Length > 2 ?
                output.Skip(2).Select(p => Path.Combine(root, p)).ToList() :
                Enumerable.Empty<string>();
        }

        private string SlnFile(string root, string name) => Path.Combine(root, $"{name}{SlnExtension}");
    }

    public class SlnCommandConfiguration
    {
        [Position(1)]
        [Help("Solution file name.")]
        public string Name { get; set; } = string.Empty;
    }
}
