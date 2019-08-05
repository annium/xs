using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Ls
{
    internal class ListCommand : Command<ListCommandConfiguration, DiscoverConfiguration>
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
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToList();

            Func<IProject, string> showProject = project => project.Name;
            if (cfg.Path)
                showProject = project => project.File;

            foreach (var project in SelectProjects(projects, cfg))
                Console.WriteLine(showProject(project));
        }

        private IEnumerable<IProject> SelectProjects(
            IEnumerable<IProject> projects,
            ListCommandConfiguration cfg
        )
        {
            // without filters - plain list
            if (!cfg.Publishable && !cfg.Testable)
                return projects;

            var filtered = new List<IProject>();
            if (cfg.Publishable)
                filtered.AddRange(projects.OfType<IPublishableProject>());
            if (cfg.Testable)
                filtered.AddRange(projects.OfType<ITestableProject>());

            return cfg.Not ? projects.Except(filtered) : filtered;
        }
    }

    internal class ListCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option]
        [Help("Show path instead of name.")]
        public bool Path { get; set; } = false;

        [Option]
        [Help("Invert selection.")]
        public bool Not { get; set; } = false;

        [Option("pub")]
        [Help("Show publishable projects.")]
        public bool Publishable { get; set; } = false;

        [Option("test")]
        [Help("Show publishable projects.")]
        public bool Testable { get; set; } = false;
    }
}