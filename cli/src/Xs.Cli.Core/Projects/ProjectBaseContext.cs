using System.Collections.Generic;
using System.IO;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Projects
{
    public abstract class ProjectBaseContext
    {
        public ProjectType Type { get; }

        public string Name { get; }

        public Version Version { get; }

        public string Description { get; }

        public FileInfo File { get; }

        public HashSet<IProject> ProjectDependencies { get; }

        public HashSet<Dependency> PackageDependencies { get; }

        public IShell Shell { get; }

        public LoggerConfiguration LoggerConfiguration { get; }

        public ILogger Logger { get; }

        public ProjectBaseContext(
            ProjectType type,
            string name,
            Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
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
            Shell = shell;
            LoggerConfiguration = loggerConfiguration;
            Logger = logger;
        }
    }
}