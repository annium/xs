using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Ls
{
    internal class ListCommand : Command<ListCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "";

        public override string Description { get; } = "List projects.";

        private readonly DiscoverProjectsTask discoverTask;

        public ListCommand(
            DiscoverProjectsTask discoverTask
        )
        {
            this.discoverTask = discoverTask;
        }

        public override void Handle(
            ListCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(cwdCfg.Cwd).FilterMask(cfg.Mask).ToArray();

            Func<IProject, string> showProject = project => project.Name;
            if (cfg.Path)
                showProject = project => project.File.FullName;

            foreach (var project in projects)
                Console.WriteLine(showProject(project));
        }
    }

    internal class ListCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Option("p")]
        [Help("Show path instead of name.")]
        public bool Path { get; set; } = false;
    }
}