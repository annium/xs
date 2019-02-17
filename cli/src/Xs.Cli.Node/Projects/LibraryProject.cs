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

namespace Xs.Cli.Node.Projects
{
    internal class LibraryProject : ProjectBase, ISpecialProject, ICleanableProject, IInstallableProject, IBuildableProject, IPublishableProject
    {
        private readonly ProjectMapper mapper;

        public LibraryProject(
            string name,
            Core.Models.Version version,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(
            Constants.ProjectType,
            name,
            version,
            file,
            projectDependencies,
            packageDependencies,
            shell,
            logger
        )
        {
            this.mapper = mapper;
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
            var fileName = $"{Name}-{version}.tgz";
            if (Name.StartsWith('@'))
            {
                var parts = Name.Substring(1).Split('/');
                fileName = $"{parts[0]}-{parts[1]}-{version}.tgz";
            }

            var file = Path.Combine(File.DirectoryName, fileName);
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);

            // for NPM, project dependencies are not swapped with package dependencies when packaged, so need to do that manually
            var projectDependencies = ProjectDependencies.ToArray();
            try
            {
                Version = version;
                ProjectDependencies.Clear();
                foreach (var dependency in projectDependencies)
                    PackageDependencies.Add(new Dependency(Constants.ProjectType, dependency.Name, version));

                Save();

                await RunAsync("pack", $"npm pack {File.DirectoryName}", token);
            }
            finally
            {
                foreach (var dependency in projectDependencies)
                {
                    ProjectDependencies.Add(dependency);
                    PackageDependencies.RemoveWhere(d => d.Name == dependency.Name);
                }

                Save();
            }

            return file;
        }

        public async Task PublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token)
        {
            var packageFile = await PackAsync(version, token);

            // due to NPM limitations, basically allowing single registry per scope, registry here is missing
            // instead, registry is specified in .npmrc
            await RunAsync("publish", $"npm publish {packageFile}", token);

            System.IO.File.Delete(packageFile);
        }

        public async Task UnpublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token)
        {
            // blank try/catch due to NPM behavior, that doesn't expect package removal, but unlisting instead
            // expecting useless package info in response
            try
            {
                await RunAsync("unpublish", $"npm unpublish {Name}@{version}", token);
            }
            catch
            {

            }
        }

        public override bool IsRelated(string path)
        {
            if (!path.StartsWith(File.DirectoryName))
                return false;

            if (Directory.Exists(path))
                return IsRelatedDirectory(new DirectoryInfo(path));

            return IsRelatedFile(new FileInfo(path));
        }

        public override void Save() => mapper.Save(this);

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