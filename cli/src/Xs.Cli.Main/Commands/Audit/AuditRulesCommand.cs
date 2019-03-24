using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;

namespace Xs.Cli.Main.Commands.Audit
{
    internal class AuditRulesCommand : Command<AuditRulesCommandConfiguration>
    {
        public override string Id { get; } = "rules";

        public override string Description { get; } = "List audit rules.";

        private readonly IAuditRule[] rules;

        private readonly ILogger logger;

        public AuditRulesCommand(
            IEnumerable<IAuditRule> rules,
            ILogger logger
        )
        {
            this.rules = rules.GroupBy(r => r.Code).Select(g => g.First()).ToArray();
            this.logger = logger;
        }

        public override void Handle(
            AuditRulesCommandConfiguration cfg,
            CancellationToken token
        )
        {
            var usedRules = (cfg.Include.Length > 0 ? rules.Where(r => cfg.Include.Contains(r.Code)) : rules)
                .Where(r => !cfg.Exclude.Contains(r.Code))
                .ToArray();

            if (usedRules.Length == 0)
            {
                Console.WriteLine("No matching rules found.");
                return;
            }

            var width = usedRules.Max(r => r.Code.Length) + 5;
            foreach (var rule in usedRules)
                Console.WriteLine($"{rule.Code.PadRight(width)}{rule.Description}");
        }
    }

    internal class AuditRulesCommandConfiguration
    {
        [Option("i")]
        [Help("Include specific rules.")]
        public string[] Include { get; set; } = Array.Empty<string>();

        [Option("e")]
        [Help("Exclude specific rules.")]
        public string[] Exclude { get; set; } = Array.Empty<string>();
    }
}