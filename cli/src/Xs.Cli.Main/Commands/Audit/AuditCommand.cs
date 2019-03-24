using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Audit;
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

        private readonly IAuditRule[] rules;

        private readonly ILogger logger;

        public AuditCommand(
            DiscoverProjectsTask discoverTask,
            IEnumerable<IAuditRule> rules,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.rules = rules.GroupBy(r => r.Code).Select(g => g.First()).ToArray();
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

            var usedRules = (cfg.Include.Length > 0 ? rules.Where(r => cfg.Include.Contains(r.Code)) : rules)
                .Where(r => !cfg.Exclude.Contains(r.Code))
                .ToArray();

            if (usedRules.Length == 0)
            {
                Console.WriteLine("No matching rules found.");
                return;
            }

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

        [Option("i")]
        [Help("Include specific rules.")]
        public string[] Include { get; set; }

        [Option("e")]
        [Help("Exclude specific rules.")]
        public string[] Exclude { get; set; }

        [Option]
        [Help("Fix errors, if possible.")]
        public bool Fix { get; set; }
    }
}