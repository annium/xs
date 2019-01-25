using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class WatchCommand : AsyncCommand<WatchCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "watch";

        public override string Description { get; } = "Watch projects' changes and install/build/test on fly.";

        private readonly IProjectFactory projectFactory;

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly ProjectsRunner runner;

        private readonly Watcher watcher;

        private readonly IShell shell;

        private readonly ILogger logger;

        private string root;

        private string mask;

        private string command;

        private bool force;

        private bool runTests;

        private CancellationToken token;

        private IProject[] projects;

        public WatchCommand(
            IProjectFactory projectFactory,
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            ProjectsRunner runner,
            Watcher watcher,
            IShell shell,
            ILogger logger
        )
        {
            this.projectFactory = projectFactory;
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.runner = runner;
            this.watcher = watcher;
            this.shell = shell;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            WatchCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            this.root = cwdCfg.Cwd;
            this.mask = cfg.Mask;
            this.command = cfg.Command;
            this.force = cfg.Force;
            this.runTests = cfg.Test;
            this.token = token;

            await Discover();

            if (command != null)
                await watcher.WatchAsync(root, FilterChange, CallCommand, CallCommand, token);
            else
                await watcher.WatchAsync(root, FilterChange, HandleChange, HandleDelete, token);
        }

        private bool FilterChange(string path) =>
        projectFactory.IsProjectFile(path) || projects.Any(e => e.IsRelated(path));

        private async Task HandleChange(string path)
        {
            var isProjectFile = projectFactory.IsProjectFile(path);
            IProject project;

            if (isProjectFile)
            {
                logger.LogInfo($"Changed project file: {path}");
                await Discover();

                project = GetProjectByPath(path);
                await InstallAsync(project, includeSelf : true);
            }
            else
                project = GetProjectByRelatedPath(path);

            if (project == null)
                return;

            logger.LogInfo($"Changed {project.Name} related file: {path}");

            await BuildAsync(project, includeSelf : true);
            if (runTests)
                await TestAsync(project, includeSelf : true);

            logger.LogInfo($"Done.");
        }

        private async Task HandleDelete(string path)
        {
            var project = GetProjectByPath(path);
            var isProjectFile = project != null;

            if (isProjectFile)
            {
                logger.LogInfo($"Deleted project file: {path}");
                await Discover();

                await InstallAsync(project, includeSelf : false);
            }
            else
                project = GetProjectByRelatedPath(path);

            if (project == null)
                return;

            logger.LogInfo($"Deleted {project.Name} related file: {path}");

            await BuildAsync(project, includeSelf: !isProjectFile);
            if (runTests)
                await TestAsync(project, includeSelf: !isProjectFile);

            logger.LogInfo($"Done.");
        }

        private Task InstallAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<IInstallableProject>(project, (p, t) => p.InstallAsync(force, t), includeSelf);

        private Task BuildAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<IBuildableProject>(project, (p, t) => p.BuildAsync(Env.Development, t), includeSelf);

        private Task TestAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<ITestableProject>(project, (p, t) => p.TestAsync(Env.Development, t), includeSelf);

        private async Task ExecuteAsync<TProject>(
            IProject project,
            Func<TProject, CancellationToken, Task> handle,
            bool includeSelf
        )
        where TProject : IProject
        {
            var selected = CollectDependants(project, includeSelf).OfType<TProject>().ToArray();

            if (selected.Length > 0)
                await runner.RunAsync(selected, handle, token);
        }

        private IEnumerable<IProject> CollectDependants(IProject project, bool includeSelf)
        {
            var list = new List<IProject>();
            if (includeSelf)
                list.Add(project);

            var dependants = projects.Where(candidate => candidate.ProjectDependencies.Contains(project)).ToArray();
            foreach (var dependant in dependants)
                list.AddRange(CollectDependants(dependant, true));

            return list.Distinct();
        }

        private Task CallCommand(string path)
        {
            var result = shell.Start(command.Replace("%", path));

            Task.Run(() => pipe(result.Output));
            Task.Run(() => pipe(result.Error));

            return result.Result;

            void pipe(StreamReader src)
            {
                while (!src.EndOfStream)
                    Console.WriteLine(src.ReadLine());
            }
        }

        private async Task Discover() =>
        projects = filterTask.Run(await discoverTask.RunAsync(root, token), mask).ToArray();

        private IProject GetProjectByPath(string path) => projects.FirstOrDefault(e => e.File.FullName == path);

        private IProject GetProjectByRelatedPath(string path) => projects.FirstOrDefault(e => e.IsRelated(path));
    }

    internal class WatchCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Option("f", isRequired : false)]
        [Help("Force install.")]
        public bool Force { get; set; } = false;

        [Option("t", isRequired : false)]
        [Help("Run tests.")]
        public bool Test { get; set; } = false;

        [Raw]
        [Help("Command to execute on change.")]
        public string Command { get; set; }
    }
}