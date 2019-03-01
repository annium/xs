using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Projects
{
    internal class LibraryProject : BaseProject, IPublishableProject
    {
        public LibraryProject(
            string name,
            Core.Models.Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
            ILogger logger
        ) : base(
            name,
            version,
            description,
            file,
            projectDependencies,
            packageDependencies,
            auditRules,
            mapper,
            shell,
            loggerConfiguration,
            logger
        ) { }

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
    }
}