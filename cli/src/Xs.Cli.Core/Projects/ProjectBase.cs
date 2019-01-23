using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;
using Xs.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public abstract class ProjectBase : IProject
    {
        public abstract ProjectType Type { get; }

        public abstract string Name { get; }

        public abstract FileInfo File { get; }

        public abstract HashSet<IProject> ProjectDependencies { get; }

        public abstract HashSet<Dependency> PackageDependencies { get; }

        private readonly IShell shell;

        private readonly ILogger logger;

        protected ProjectBase(
            IShell shell,
            ILogger logger
        )
        {
            this.shell = shell;
            this.logger = logger;
        }

        public abstract bool IsRelated(string path);

        public abstract void Save();

        protected async Task RunAsync(string operation, string command, CancellationToken token)
        {
            logger.LogInfo($"Start {Name} {operation}.");

            var result = await shell.RunAsync(command, token);

            if (result.Code == 0)
                logger.LogInfo($"Finished {Name} {operation}.");
            else
                throw new Exception($"Failed {Name} {operation}:{Environment.NewLine}{result.Output}.");
        }
    }
}