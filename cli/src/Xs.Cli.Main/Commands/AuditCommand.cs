using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands
{
    internal class AuditCommand : AsyncCommand<CwdCommandConfiguration>
    {
        public override string Id { get; } = "audit";

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

        public override async Task HandleAsync(
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = (await discoverTask.RunAsync(cwdCfg.Cwd))
                .OfType<IAuditableProject>()
                .ToArray();
            logger.LogDebug($"Audit {projects.Length} projects.");

            var results = await Task.WhenAll(projects.Select(async project =>
            {
                var errors = await project.AuditAsync(token);
                return (project, errors);
            }));

            var sb = new StringBuilder();
            foreach (var(project, errors) in results)
            {
                var errorsCount = errors.Count();
                if (errorsCount > 0)
                {
                    sb.AppendLine($"{project}: {errorsCount} error(s):");
                    foreach (var error in errors)
                        sb.AppendLine($" - {error}");
                }
            }
            Console.Write(sb.ToString());
        }
    }
}