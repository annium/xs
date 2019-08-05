using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class FormatCommand : Command<DiscoverConfiguration>
    {
        public override string Id { get; } = "format";
        public override string Description { get; } = "Format projects.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger<FormatCommand> logger;

        public FormatCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<FormatCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override void Handle(
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg).ToArray();

            foreach (var project in projects)
                project.Save();
        }
    }
}