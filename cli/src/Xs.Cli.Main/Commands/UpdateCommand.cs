using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands
{
    internal class UpdateCommand : Command<UpdateCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "update";

        public override string Description { get; } = "Update dependencies in projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly ILogger logger;

        public UpdateCommand(
            DiscoverProjectsTask discoverTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            UpdateCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var allProjects = discoverTask.Run(cwdCfg.Cwd);
            var dependencies = allProjects.SelectMany(e => e.PackageDependencies).Distinct().ToArray();
            var targets = allProjects.FilterMask(cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.LogInfo($"No projects found to update.");
                return;
            }
            logger.LogDebug($"Update dependencies in {targets.Length} projects.");
        }
    }

    internal class UpdateCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";
    }
}