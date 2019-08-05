using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class WatchCommand : AsyncCommand<WatchCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "watch";
        public override string Description { get; } = "Watch projects' changes and install/build/test on fly.";
        private readonly IProjectFactory projectFactory;
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly Watcher watcher;
        private readonly IShell shell;
        private readonly ILogger<WatchCommand> logger;
        private readonly LoggerConfiguration loggerConfiguration;
        private string mask;
        private ProjectType type;
        private string command;
        private bool force;
        private bool runTests;
        private string testFilter;
        private DiscoverConfiguration discoverCfg;
        private CancellationToken token;
        private IProject[] projects;

        public WatchCommand(
            IProjectFactory projectFactory,
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            Watcher watcher,
            IShell shell,
            ILogger<WatchCommand> logger,
            LoggerConfiguration loggerConfiguration
        )
        {
            this.projectFactory = projectFactory;
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.watcher = watcher;
            this.shell = shell;
            this.logger = logger;
            this.loggerConfiguration = loggerConfiguration;
        }

        public override async Task HandleAsync(
            WatchCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            this.mask = cfg.Mask;
            this.type = cfg.Type;
            this.command = cfg.Command;
            this.force = cfg.Force;
            this.runTests = cfg.Test || !string.IsNullOrWhiteSpace(cfg.TestFilter);
            this.testFilter = cfg.TestFilter;
            this.discoverCfg = discoverCfg;
            this.token = token;

            Discover();

            if (string.IsNullOrWhiteSpace(command))
                await watcher.WatchAsync(discoverCfg.Root, FilterChange, HandleChange, HandleDelete, token);
            else
                await watcher.WatchAsync(discoverCfg.Root, FilterChange, CallCommand, CallCommand, token);
        }

        private bool FilterChange(string path) =>
        projectFactory.IsProjectFile(path) || projects.Any(e => e.IsRelated(path));

        private async Task HandleChange(string path)
        {
            var isProjectFile = projectFactory.IsProjectFile(path);
            IProject project;

            if (isProjectFile)
            {
                logger.Info($"Changed project file: {path}");
                Discover();

                project = GetProjectByPath(path);
                await InstallAsync(project, includeSelf : true);
            }
            else
                project = GetProjectByRelatedPath(path);

            if (project == null)
                return;

            logger.Info($"Changed {project} related file: {path}");

            await BuildAsync(project, includeSelf : true);
            if (runTests)
                await TestAsync(project, includeSelf : true);

            logger.Info($"Done.");
        }

        private async Task HandleDelete(string path)
        {
            var project = GetProjectByPath(path);
            var isProjectFile = project != null;

            if (isProjectFile)
            {
                logger.Info($"Deleted project file: {path}");
                Discover();

                await InstallAsync(project, includeSelf : false);
            }
            else
                project = GetProjectByRelatedPath(path);

            if (project == null)
                return;

            logger.Info($"Deleted {project} related file: {path}");

            await BuildAsync(project, includeSelf: !isProjectFile);
            if (runTests)
                await TestAsync(project, includeSelf: !isProjectFile);

            logger.Info($"Done.");
        }

        private Task InstallAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<IInstallableProject>(project, (p, t) => p.InstallAsync(force, t), includeSelf);

        private Task BuildAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<IBuildableProject>(project, (p, t) => p.BuildAsync(Env.Development, t), includeSelf);

        private Task TestAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<ITestableProject>(project, (p, t) => p.TestAsync(Env.Development, this.testFilter, t), includeSelf);

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

            var dependants = projects.Where(candidate => candidate.Projects.Any(d => d.Value == project)).ToArray();
            foreach (var dependant in dependants)
                list.AddRange(CollectDependants(dependant, true));

            return list.Distinct();
        }

        private Task CallCommand(string path)
        {
            var result = shell
                .Cmd(command.Replace("%", path))
                .Pipe(loggerConfiguration.LogLevel <= LogLevel.Debug)
                .Start();

            Task.Run(() => pipe(result.Output));
            Task.Run(() => pipe(result.Error));

            return result.Result;

            void pipe(StreamReader src)
            {
                while (!src.EndOfStream)
                    Console.WriteLine(src.ReadLine());
            }
        }

        private void Discover() => projects = discoverTask.Run(discoverCfg)
        .FilterMask(mask)
        .FilterType(type)
        .OrderByDescending(p => p.Name.Length)
        .ToArray();

        private IProject GetProjectByPath(string path) => projects.FirstOrDefault(e => e.File == path);

        private IProject GetProjectByRelatedPath(string path) => projects.FirstOrDefault(e => e.IsRelated(path));
    }

    internal class WatchCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option("f", isRequired : false)]
        [Help("Force install.")]
        public bool Force { get; set; } = false;

        [Option("t", isRequired : false)]
        [Help("Run tests.")]
        public bool Test { get; set; } = false;

        [Option("tf", isRequired : false)]
        [Help("Tests filter.")]
        public string TestFilter { get; set; } = string.Empty;

        [Raw]
        [Help("Command to execute on change.")]
        public string Command { get; set; } = string.Empty;
    }
}