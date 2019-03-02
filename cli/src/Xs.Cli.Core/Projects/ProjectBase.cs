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

        protected readonly LoggerConfiguration loggerConfiguration;

        protected readonly ILogger logger;

        protected ProjectBase(ProjectBaseContext context)
        {
            Type = context.Type;
            Name = context.Name;
            Version = context.Version;
            Description = context.Description;
            File = context.File;
            ProjectDependencies = context.ProjectDependencies;
            PackageDependencies = context.PackageDependencies;
            shell = context.Shell;
            loggerConfiguration = context.LoggerConfiguration;
            logger = context.Logger;
        }

        public bool IsRelated(string path)
        {
            if (!path.StartsWith(File.DirectoryName))
                return false;

            return IsRelated(new FileInfo(path));
        }

        public abstract void Save();

        public override string ToString() => Name;

        protected abstract bool IsRelated(FileInfo file);

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