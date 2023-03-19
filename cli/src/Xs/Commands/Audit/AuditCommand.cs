using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands.Audit;

internal class AuditCommand : Command<AuditCommandConfiguration, DiscoverConfiguration>, ILogSubject<AuditCommand>
{
    public override string Id => "";
    public override string Description => "Audit projects.";
    public ILogger<AuditCommand> Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IAuditRule[] _rules;

    public AuditCommand(
        DiscoverProjectsTask discoverTask,
        IEnumerable<IAuditRule> rules,
        ILogger<AuditCommand> logger
    )
    {
        _discoverTask = discoverTask;
        _rules = rules.GroupBy(r => r.Code).Select(g => g.First()).ToArray();
        Logger = logger;
    }

    public override void Handle(
        AuditCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var projects = _discoverTask.RunAsync(discoverCfg).Await()
            .ToArray();
        var auditedProjects = projects
            .FilterMask(cfg.Mask)
            .OfType<IAuditableProject>()
            .ToArray();
        this.Log().Debug($"Audit {auditedProjects.Length} projects.");

        var usedRules = (cfg.Include.Length > 0 ? _rules.Where(r => cfg.Include.Contains(r.Code)) : _rules)
            .Where(r => !cfg.Exclude.Contains(r.Code))
            .Select(r => r.Code)
            .ToArray();

        if (usedRules.Length == 0)
        {
            Console.WriteLine("No matching rules found.");
            return;
        }

        this.Log().Debug($"Use {usedRules.Length} rule(s):");
        foreach (var rule in usedRules)
            this.Log().Debug(rule);

        foreach (var project in auditedProjects)
        {
            var results = project.Audit(projects, usedRules, cfg.Fix, ct);
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