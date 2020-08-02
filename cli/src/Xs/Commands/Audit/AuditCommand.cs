using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands.Audit
{
    internal class AuditCommand : Command<AuditCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "";

        public override string Description { get; } = "Audit projects.";

        private readonly DiscoverProjectsTask _discoverTask;

        private readonly IAuditRule[] _rules;

        private readonly ILogger<AuditCommand> _logger;

        public AuditCommand(
            DiscoverProjectsTask discoverTask,
            IEnumerable<IAuditRule> rules,
            ILogger<AuditCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _rules = rules.GroupBy(r => r.Code).Select(g => g.First()).ToArray();
            _logger = logger;
        }

        public override void Handle(
            AuditCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = _discoverTask.Run(discoverCfg)
                .ToArray();
            var auditedProjects = projects
                .FilterMask(cfg.Mask)
                .OfType<IAuditableProject>()
                .ToArray();
            _logger.Debug($"Audit {auditedProjects.Length} projects.");

            var usedRules = (cfg.Include.Length > 0 ? _rules.Where(r => cfg.Include.Contains(r.Code)) : _rules)
                .Where(r => !cfg.Exclude.Contains(r.Code))
                .Select(r => r.Code)
                .ToArray();

            if (usedRules.Length == 0)
            {
                Console.WriteLine("No matching rules found.");
                return;
            }

            _logger.Debug($"Use {usedRules.Length} rule(s):");
            foreach (var rule in usedRules)
                _logger.Debug(rule);

            foreach (var project in auditedProjects)
            {
                var results = project.Audit(projects, usedRules, cfg.Fix, token);
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
        [Position(1, isRequired: false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Option("i")]
        [Help("Include specific rules.")]
        public string[] Include { get; set; } = Array.Empty<string>();

        [Option("e")]
        [Help("Exclude specific rules.")]
        public string[] Exclude { get; set; } = Array.Empty<string>();

        [Option]
        [Help("Fix errors, if possible.")]
        public bool Fix { get; set; }
    }
}