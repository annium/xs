using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Xs.Cli.Core.Audit;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;

namespace Annium.Xs.Cli.Commands.Audit;

internal class AuditCommand
    : AsyncCommand<AuditCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "";
    public static string Description => "Audit projects.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IAuditRule[] _rules;

    public AuditCommand(DiscoverProjectsTask discoverTask, IEnumerable<IAuditRule> rules, ILogger logger)
    {
        _discoverTask = discoverTask;
        _rules = rules.GroupBy(r => r.Code).Select(g => g.First()).ToArray();
        Logger = logger;
    }

    public override async Task HandleAsync(
        AuditCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var projects = await _discoverTask.RunAsync(discoverCfg);
        var auditedProjects = projects.FilterMask(cfg.Mask).OfType<IAuditableProject>().ToArray();
        this.Debug("Audit {length} projects.", auditedProjects.Length);

        var usedRules = (cfg.Include.Length > 0 ? _rules.Where(r => cfg.Include.Contains(r.Code)) : _rules)
            .Where(r => !cfg.Exclude.Contains(r.Code))
            .Select(r => r.Code)
            .ToArray();

        if (usedRules.Length == 0)
        {
            Console.WriteLine("No matching rules found.");
            return;
        }

        this.Debug("Use {length} rule(s):", usedRules.Length);
        foreach (var rule in usedRules)
            this.Debug<string>("{rule}", rule);

        foreach (var project in auditedProjects)
        {
            var results = project.Audit(projects, usedRules, cfg.Fix, ct);
            if (results.Count > 0)
            {
                Console.WriteLine($"{project}: {results.Count} result(s):");
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
    public string[] Include { get; set; } = [];

    [Option("e")]
    [Help("Exclude specific rules.")]
    public string[] Exclude { get; set; } = [];

    [Option]
    [Help("Fix errors, if possible.")]
    public bool Fix { get; set; }
}
