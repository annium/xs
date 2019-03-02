using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands
{
    internal class AuditCommand : Command<AuditCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "audit";

        public override string Description { get; } = "Audit projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly ILogger logger;

        public AuditCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.logger = logger;
        }

        public override void Handle(
            AuditCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = filterTask.Run(discoverTask.Run(cwdCfg.Cwd), cfg.Mask)
                .OfType<IAuditableProject>()
                .ToArray();
            logger.LogDebug($"Audit {projects.Length} projects.");

            foreach (var project in projects)
            {
                var results = project.Audit(cfg.Fix, token);
                if (results.Length > 0)
                {
                    Console.WriteLine($"{project}: {results.Length} result(s):");
                    foreach (var result in results)
                        Console.WriteLine($" - {result.Message} (" + (result.IsFixed ? "fixed" : "not fixed") + ")");
                }
            }
        }
    }

    internal class AuditCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Option]
        [Help("Fix errors, if possible.")]
        public bool Fix { get; set; }
    }
}