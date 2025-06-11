using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Xs.Cli.Core.Audit;

namespace Annium.Xs.Cli.Commands.Audit;

internal class AuditRulesCommand : Command<AuditRulesCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "rules";
    public static string Description => "List audit rules.";
    private readonly IAuditRule[] _rules;

    public AuditRulesCommand(IEnumerable<IAuditRule> rules)
    {
        _rules = rules.GroupBy(r => r.Code).Select(g => g.First()).ToArray();
    }

    public override void Handle(AuditRulesCommandConfiguration cfg, CancellationToken ct)
    {
        var usedRules = (cfg.Include.Length > 0 ? _rules.Where(r => cfg.Include.Contains(r.Code)) : _rules)
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
    public string[] Include { get; set; } = [];

    [Option("e")]
    [Help("Exclude specific rules.")]
    public string[] Exclude { get; set; } = [];
}
