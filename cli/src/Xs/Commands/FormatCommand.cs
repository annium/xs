using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class FormatCommand : Command<FormatCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "format";
        public override string Description { get; } = "Format projects.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger<FormatCommand> logger;

        public FormatCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<FormatCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override void Handle(
            FormatCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();

            logger.Debug($"Format {projects} project(s)");
            foreach (var project in projects)
            {
                logger.Debug($"Format {project}");
                project.Save();
            }
        }
    }

    internal class FormatCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }
    }
}