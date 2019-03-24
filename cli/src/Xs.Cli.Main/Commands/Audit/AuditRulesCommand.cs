using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;

namespace Xs.Cli.Main.Commands.Audit
{
    internal class AuditRulesCommand : Command
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
            CancellationToken token
        )
        {
            var width = rules.Max(r => r.Code.Length) + 5;
            foreach (var rule in rules)
                Console.WriteLine($"{rule.Code.PadRight(width)}{rule.Description}");
        }
    }
}