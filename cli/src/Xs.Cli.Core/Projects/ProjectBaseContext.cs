using System.Collections.Generic;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public abstract class ProjectBaseContext<TProject> where TProject : class, IProject
{
    public ProjectType Type { get; }
    public string Name { get; }
    public Version Version { get; }
    public string Description { get; }
    public string Directory { get; }
    public HashSet<Dependency<IProject>> Projects { get; }
    public HashSet<Dependency<Package>> Packages { get; }
    public IShell Shell { get; }
    public LoggerConfiguration LoggerConfiguration { get; }
    public ILogger<TProject> Logger { get; }

    public ProjectBaseContext(
        ProjectType type,
        string name,
        Version version,
        string description,
        string directory,
        HashSet<Dependency<IProject>> projects,
        HashSet<Dependency<Package>> packages,
        IShell shell,
        LoggerConfiguration loggerConfiguration,
        ILogger<TProject> logger
    )
    {
        Type = type;
        Name = name;
        Version = version;
        Description = description;
        Directory = directory;
        Projects = projects;
        Packages = packages;
        Shell = shell;
        LoggerConfiguration = loggerConfiguration;
        Logger = logger;
    }
}