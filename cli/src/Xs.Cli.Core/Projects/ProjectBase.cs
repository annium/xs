using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using SysDirectory = System.IO.Directory;
using SysFile = System.IO.File;

namespace Xs.Cli.Core.Projects
{
    public abstract class ProjectBase<TProject> : IProject where TProject : ProjectBase<TProject>
    {
        public ProjectType Type { get; }
        public string Name { get; private set; }
        public Models.Version Version { get; private set; }
        public string Description { get; private set; }
        public string Directory { get; private set; }
        public abstract string File { get; }
        public HashSet<Dependency<IProject>> Projects { get; }
        public HashSet<Dependency<Package>> Packages { get; }
        protected readonly IShell shell;
        protected readonly LoggerConfiguration loggerConfiguration;
        protected readonly ILogger<ProjectBase<TProject>> logger;
        private string currentDirectory;
        private string currentName;

        protected ProjectBase(ProjectBaseContext<TProject> context)
        {
            Type = context.Type;
            Name = currentName = context.Name;
            Version = context.Version;
            Description = context.Description;
            Directory = currentDirectory = context.Directory;
            Projects = context.Projects;
            Packages = context.Packages;
            shell = context.Shell;
            loggerConfiguration = context.LoggerConfiguration;
            logger = context.Logger;
        }

        public void SetName(string name)
        {
            Name = name;
            Directory = FixProjectDirectory(Directory);
        }

        public void SetVersion(Models.Version version)
        {
            Version = version;
        }

        public void SetDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));

            Directory = FixProjectDirectory(directory);
        }

        public bool IsRelated(string path)
        {
            if (!path.StartsWith(Directory))
                return false;

            return IsRelated(new FileInfo(path));
        }

        public void Save()
        {
            // sync directory
            if (Directory != currentDirectory)
            {
                // ensure target doesn't exist
                if (SysDirectory.Exists(Directory) || SysFile.Exists(Directory))
                    throw new InvalidOperationException($"{Directory} already exists.");

                // create parent directory, if needed
                var parentDirectory = Path.GetDirectoryName(Directory);
                if (!SysDirectory.Exists(parentDirectory))
                {
                    logger.Trace($"Create {Name} missing target parent directory {parentDirectory}");
                    SysDirectory.CreateDirectory(parentDirectory);
                }

                logger.Debug($"Move {Name} to {Directory}");

                SysDirectory.Move(currentDirectory, Directory);

                currentDirectory = Directory;
            }

            // sync name
            if (Name != currentName)
            {
                OnNameChangeSave(currentName, Name);

                currentName = Name;
            }

            // call implementation-specific save logic 
            HandleSave();
        }

        public override string ToString() => Name;

        protected abstract void HandleSave();

        protected abstract bool IsRelated(FileInfo file);

        protected virtual string FixProjectDirectory(string directory) => directory;

        protected virtual void OnNameChangeSave(string oldName, string newName) { }

        protected void DeleteDirectory(string path)
        {
            path = Path.Combine(Directory, path);
            if (SysDirectory.Exists(path))
                SysDirectory.Delete(path, recursive : true);
        }

        protected void DeleteFiles(string mask)
        {
            foreach (var file in SysDirectory.GetFiles(Directory, mask, SearchOption.TopDirectoryOnly))
                SysFile.Delete(file);
        }

        protected async Task RunAsync(string operation, string command, CancellationToken token)
        {
            logger.Info($"Start {Name} {operation}.");

            var result = await shell
                .Cmd(command)
                .Configure(new ProcessStartInfo() { WorkingDirectory = Directory })
                .Pipe(loggerConfiguration.LogLevel <= LogLevel.Debug)
                .RunAsync(token);

            if (result.IsSuccess)
                logger.Info($"Finished {Name} {operation}.");
            else
                throw new Exception($"Failed {Name} {operation}:{Environment.NewLine}{result.Output}{Environment.NewLine}{result.Error}");
        }
    }
}