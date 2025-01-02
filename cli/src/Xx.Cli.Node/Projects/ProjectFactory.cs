using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Shell;
using Annium.Logging;
using Xx.Cli.Core.Audit;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Logging;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Xx.Cli.Core.Tools;
using Xx.Cli.Node.Tools;

namespace Xx.Cli.Node.Projects;

internal class ProjectFactory : PlatformProjectFactoryBase, IPlatformProjectFactory
{
    public ProjectType Type => Constants.ProjectType;
    public static readonly string[] TrackedFileExtensions = { ".html", ".ts", ".tsx", ".js", ".scss", ".css", ".json" };
    public static readonly string[] IgnoredFolders = { "build", "dist", ModulesDirectory };
    public const string ModulesDirectory = "node_modules";
    public const string ProjectFileName = "package.json";
    public const string LockFileName = "pnpm-lock.yaml";
    private readonly IEnumerable<IAuditRule<IPlatformProject>> _auditRules;
    private readonly ProjectMapper _mapper;
    private readonly LoggerConfiguration _loggerConfiguration;
    private readonly IShell _shell;
    private readonly IServiceProvider _provider;

    public ProjectFactory(
        IEnumerable<IAuditRule<IPlatformProject>> auditRules,
        ProjectMapper mapper,
        LoggerConfiguration loggerConfiguration,
        IShell shell,
        IServiceProvider provider
    )
    {
        _auditRules = auditRules;
        _mapper = mapper;
        _loggerConfiguration = loggerConfiguration;
        _shell = shell;
        _provider = provider;
    }

    public bool IsProjectDirectory(string directory)
    {
        // considered project directory, if path doesn't contain modulesDirectory
        // and it's only one in all subdirectories, except those in modulesDirectory
        if (!Directory.Exists(directory))
            return false;

        if (directory.Contains(ModulesDirectory))
            return false;

        var projectFiles = Directory.GetFiles(directory, ProjectFileName);
        if (projectFiles.Length != 1)
            return false;

        return !FileManager.FindDirectory(directory, IsMatch, IgnoredFolders);

        static bool IsMatch(string dir) => Directory.GetFiles(dir, ProjectFileName).Length > 0;
    }

    public bool IsProjectFile(string file)
    {
        if (!file.EndsWith(ProjectFileName))
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
        var file = new FileInfo(Path.Combine(directory, ProjectFileName));
        var (name, version, description, projectDeps, packageDeps, scripts, isPackable) = _mapper.Load(
            file.FullName,
            discoverCfg
        );

        var projectDependencies = projectDeps.Select(e => GetProjectDependencyMock(file, e)).ToHashSet();

        var packageDependencies = packageDeps.ToHashSet();

        var isTestProject = scripts.ContainsKey("test");

        if (isPackable && isTestProject)
            return new LibraryTestProject(GetContext<LibraryTestProject>());

        if (isPackable)
            return new LibraryProject(GetContext<LibraryProject>());

        if (isTestProject)
            return new TestProject(GetContext<TestProject>());

        return new SealedProject(GetContext<SealedProject>());

        PlatformProjectContext GetContext<TProject>()
            where TProject : PlatformProject =>
            new(
                Constants.ProjectType,
                name,
                version,
                description,
                directory,
                projectDependencies,
                packageDependencies,
                scripts,
                _shell,
                _loggerConfiguration,
                _provider.Resolve<ILogger>(),
                _auditRules,
                projectCfg as PlatformConfiguration ?? new PlatformConfiguration(),
                _mapper
            );
    }
}
