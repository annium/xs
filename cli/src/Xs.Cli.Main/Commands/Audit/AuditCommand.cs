using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Audit
{
    internal class AuditCommand : Command<AuditCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "";

        public override string Description { get; } = "Audit projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly ILogger logger;

        public AuditCommand(
            DiscoverProjectsTask discoverTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            AuditCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg)
                .ToArray();
            var auditedProjects = projects
                .FilterMask(cfg.Mask)
                .OfType<IAuditableProject>()
                .ToArray();
            logger.Debug($"Audit {auditedProjects.Length} projects.");

            foreach (var project in auditedProjects)
            {
                var results = project.Audit(projects, cfg.Fix, token);
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