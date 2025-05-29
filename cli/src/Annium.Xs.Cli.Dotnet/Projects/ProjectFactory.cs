using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Shell;
using Annium.Linq;
using Annium.Logging;
using Annium.Xs.Cli.Core.Audit;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Logging;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Dotnet.Tools;

namespace Annium.Xs.Cli.Dotnet.Projects;

internal class ProjectFactory : PlatformProjectFactoryBase, IPlatformProjectFactory, ILogSubject
{
    public const string ProjectFileExtension = ".csproj";
    public const string TestCoveragePackage = "coverlet.msbuild";
    public static readonly string[] TrackedFileExtensions = [".cs"];
    public static readonly string[] IgnoredFolders = ["bin", "obj"];
    private const string ProjectFileMask = "*.csproj";
    public ILogger Logger { get; }
    public ProjectType Type => Constants.ProjectType;
    private readonly IEnumerable<IAuditRule<IPlatformProject>> _auditRules;
    private readonly ProjectMapper _mapper;
    private readonly IShell _shell;
    private readonly LoggerConfiguration _loggerConfiguration;
    private readonly IServiceProvider _provider;

    public ProjectFactory(
        IServiceProvider provider,
        IEnumerable<IAuditRule<IPlatformProject>> auditRules,
        ProjectMapper mapper,
        IShell shell,
        LoggerConfiguration loggerConfiguration,
        ILogger logger
    )
    {
        Logger = logger;
        _auditRules = auditRules;
        _mapper = mapper;
        _shell = shell;
        _loggerConfiguration = loggerConfiguration;
        _provider = provider;
    }

    public bool IsProjectDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            this.Trace<string>("Directory {directory} doesn't exist", directory);
            return false;
        }

        var projectFiles = Directory.GetFiles(directory, ProjectFileMask);
        if (projectFiles.Length != 1)
        {
            this.Trace<string, int, string>(
                "Directory {directory} contains {count}: {list} project files",
                directory,
                projectFiles.Length,
                projectFiles.Join(", ")
            );
            return false;
        }

        if (FileManager.FindDirectory(directory, ContainsProjectFiles, IgnoredFolders))
        {
            this.Trace<string>("Directory {directory} contains other project files in it's tree", directory);
            return false;
        }

        return true;

        static bool ContainsProjectFiles(string dir) => Directory.GetFiles(dir, ProjectFileMask).Length > 0;
    }

    public bool IsProjectFile(string file)
    {
        if (!file.EndsWith(ProjectFileExtension))
            return false;

        var parent =
            Directory.GetParent(file) ?? throw new DirectoryNotFoundException($"File {file} has no parent directory");
        var directory = parent.FullName;
        if (FileManager.IsUnrootedDirectoryIgnored(directory, IgnoredFolders))
            return false;

        return IsProjectDirectory(directory);
    }

    public IProject CreateProject(
        string directory,
        DiscoverConfiguration discoverCfg,
        PlatformConfigurationBase? projectCfg
    )
    {
        var file = new FileInfo(Directory.GetFiles(directory, ProjectFileMask, SearchOption.TopDirectoryOnly).First());
        var (
            name,
            description,
            solutions,
            targetFramework,
            outputType,
            projectDeps,
            packageDeps,
            isPackable,
            isTestProject
        ) = _mapper.Load(file.FullName, discoverCfg);

        var projectDependencies = projectDeps.Select(e => GetProjectDependencyMock(file, e)).ToHashSet();

        var packageDependencies = packageDeps.ToHashSet();

        if (isPackable)
            return new LibraryProject(GetContext());

        if (isTestProject)
            return new TestProject(GetContext());

        return new InternalProject(GetContext());

        PlatformProjectContext GetContext() =>
            new(
                Constants.ProjectType,
                name,
                description,
                directory,
                projectDependencies,
                packageDependencies,
                projectCfg as PlatformConfiguration ?? new PlatformConfiguration(),
                solutions,
                targetFramework,
                outputType,
                _auditRules,
                _mapper,
                _shell,
                _loggerConfiguration,
                _provider.Resolve<ILogger>()
            );
    }
}
