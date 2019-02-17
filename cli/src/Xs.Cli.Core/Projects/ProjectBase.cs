using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Projects
{
    public abstract class ProjectBase : IProject
    {
        public ProjectType Type { get; }

        public string Name { get; }

        public Models.Version Version { get; protected set; }

        public string Description { get; }

        public FileInfo File { get; }

        public HashSet<IProject> ProjectDependencies { get; }

        public HashSet<Dependency> PackageDependencies { get; }

        protected readonly IShell shell;

        protected readonly ILogger logger;

        protected ProjectBase(
            ProjectType type,
            string name,
            Models.Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            IShell shell,
            ILogger logger
        )
        {
            Type = type;
            Name = name;
            Version = version;
            Description = description;
            File = file;
            ProjectDependencies = projectDependencies;
            PackageDependencies = packageDependencies;
            this.shell = shell;
            this.logger = logger;
        }

        public abstract bool IsRelated(string path);

        public abstract void Save();

        public override string ToString() => Name;

        protected void DeleteDirectory(string path)
        {
            path = Path.Combine(File.DirectoryName, path);
            if (Directory.Exists(path))
                Directory.Delete(path, recursive : true);
        }

        protected void DeleteFiles(string mask)
        {
            foreach (var file in Directory.GetFiles(File.DirectoryName, mask, SearchOption.TopDirectoryOnly))
                System.IO.File.Delete(file);
        }

        protected async Task RunAsync(string operation, string command, CancellationToken token)
        {
            logger.LogInfo($"Start {Name} {operation}.");

            var result = await shell.RunAsync(
                new ProcessStartInfo() { WorkingDirectory = File.Directory.FullName },
                command, token);

            if (result.IsSuccess)
                logger.LogInfo($"Finished {Name} {operation}.");
            else
                throw new Exception($"Failed {Name} {operation}:{Environment.NewLine}{result.Output}{Environment.NewLine}{result.Error}");
        }
    }
}