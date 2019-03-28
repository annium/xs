using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class LibraryProject : BaseProject, IPublishableProject
    {
        public LibraryProject(SpecialProjectContext context) : base(context) { }

        public async Task<string> PackAsync(Core.Models.Version version, CancellationToken token)
        {
            await BuildAsync(Env.Production, token);

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
            var projectDependencies = Projects.ToArray();
            try
            {
                Version = version;
                Projects.Clear();
                foreach (var(type, dependency) in projectDependencies)
                    Packages.Add(new Dependency<Package>(type, new Package(Constants.ProjectType, dependency.Name, version)));

                Save();

                await RunAsync("pack", $"npm pack {File.DirectoryName}", token);
            }
            finally
            {
                foreach (var dependency in projectDependencies)
                {
                    Projects.Add(dependency);
                    Packages.RemoveWhere(d => d.Type == dependency.Type && d.Value.Name == dependency.Value.Name);
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