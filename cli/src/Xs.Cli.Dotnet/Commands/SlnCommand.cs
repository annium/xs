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
            var projects = discoverTask.Run(discoverCfg)
                .OfType<ISpecialProject>()
                .ToArray();

            var slnFile = Path.Combine(root, $"{cfg.Name}{SlnExtension}");
            logger.Debug($"Write solution file {slnFile}");
            await shell.Cmd($"dotnet new sln --name {cfg.Name} --output {root}").RunAsync();

            foreach (var project in projects)
            {
                var folder = Path.GetRelativePath(root, Directory.GetParent(project.Directory).FullName);
                logger.Debug($"Add {project} to solution file at {folder}");
                await shell.Cmd($"dotnet sln {slnFile} add --solution-folder {folder} {project.File}").RunAsync();

            }
        }
    }

    public class SlnCommandConfiguration
    {
        [Position(1)]
        [Help("Solution file name.")]
        public string Name { get; set; } = string.Empty;
    }
}