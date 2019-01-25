using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Core.Models;

namespace Xs.Cli.Node.Projects
{
    internal class LibraryProject : ProjectBase, ISpecialProject, ICleanableProject, IInstallableProject, IBuildableProject, IPublishableProject
    {
        public override ProjectType Type { get; } = Constants.ProjectType;

        public override string Name { get; }

        public override FileInfo File { get; }

        public override HashSet<IProject> ProjectDependencies { get; }

        public override HashSet<Dependency> PackageDependencies { get; }

        public Core.Models.Version Version { get; set; }

        private readonly ProjectMapper mapper;

        protected readonly IShell shell;

        protected readonly ILogger logger;

        public LibraryProject(
            string name,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            Core.Models.Version version,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(shell, logger)
        {
            Name = name;
            File = file;
            ProjectDependencies = projectDependencies;
            PackageDependencies = packageDependencies;
            Version = version;
            this.mapper = mapper;
            this.shell = shell;
            this.logger = logger;
        }

        public Task CleanAsync(CancellationToken token)
        {
            logger.LogInfo($"Start {Name} clean.");

            DeleteDirectory(ProjectFactory.ModulesDirectory);

            DeleteFiles("*.tgz");

            logger.LogInfo($"Finished {Name} clean.");

            return Task.CompletedTask;
        }

        public Task InstallAsync(bool force, CancellationToken token)
        {
            var forceFlag = force ? "--force" : string.Empty;

            return RunAsync("install", $"yarn install {forceFlag} --no-emoji --no-progress", token);
        }

        public Task BuildAsync(Env env, CancellationToken token) =>
            RunAsync("build", "yarn run build", token);

        public async Task<string> PackAsync(Core.Models.Version version, CancellationToken token)
        {
            // TODO: version needs to be in package.json
            var fileName = $"{Name}-{version}.tgz";
            if (Name.StartsWith('@'))
            {
                var parts = Name.Substring(1).Split('/');
                fileName = $"{parts[0]}-{parts[1]}-{version}.tgz";
            }

            var file = Path.Combine(File.DirectoryName, fileName);
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);

            await RunAsync("pack", $"npm pack {File.DirectoryName}", token);

            return file;
        }

        public async Task PublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token)
        {
            var packageFile = await PackAsync(version, token);

            // TODO: set registry in .npmrc to publish
            await RunAsync("publish", $"npm publish {packageFile}", token);

            System.IO.File.Delete(packageFile);
        }

        public Task UnpublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token) =>
            RunAsync("unpublish", $"npm unpublish {Name}@{version}", token);

        public override bool IsRelated(string path)
        {
            if (!path.StartsWith(File.DirectoryName))
                return false;

            if (Directory.Exists(path))
                return IsRelatedDirectory(new DirectoryInfo(path));

            return IsRelatedFile(new FileInfo(path));
        }

        public override void Save() => mapper.Save(this);

        public override string ToString() => Name;

        private bool IsRelatedDirectory(DirectoryInfo directory)
        {
            return directory.GetFiles("*", SearchOption.AllDirectories).Any(IsRelatedFile);
        }

        private bool IsRelatedFile(FileInfo file) =>
            (
                file.FullName.EndsWith(ProjectFactory.ProjectFileName) ||
                ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith)
            ) &&
            !ProjectFactory.IgnoredFolders.Any(file.FullName.Contains);
    }
}