using System.Linq;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands
{
    internal class FormatCommand : Command<FormatCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "format";
        public override string Description { get; } = "Format projects.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ILogger<FormatCommand> _logger;

        public FormatCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<FormatCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _logger = logger;
        }

        public override void Handle(
            FormatCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = _discoverTask.RunAsync(discoverCfg).Await()
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();

            _logger.Debug($"Format {projects} project(s)");
            foreach (var project in projects)
            {
                _logger.Debug($"Format {project}");
                project.Save();
            }
        }
    }

    internal class FormatCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired: false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; } = ProjectType.None;
    }
}