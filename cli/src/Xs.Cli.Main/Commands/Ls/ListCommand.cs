using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Ls
{
    internal class ListCommand : AsyncCommand<ListCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "";

        public override string Description { get; } = "list projects";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        public ListCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
        }

        public override async Task HandleAsync(
            ListCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = await discoverTask.RunAsync(cwdCfg.Cwd);
            projects = filterTask.Run(projects, cfg.Mask);

            foreach (var project in projects)
                Console.WriteLine(project.Name);
        }
    }

    internal class ListCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask")]
        public string Mask { get; set; } = "*";
    }
}