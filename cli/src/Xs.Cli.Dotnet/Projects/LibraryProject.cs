using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Dotnet.Projects
{
    internal class LibraryProject : BaseProject, IPublishableProject
    {
        public LibraryProject(SpecialProjectContext context) : base(context) { }

        public async Task<string> PackAsync(Core.Models.Version version, CancellationToken token)
        {
            var file = Path.Combine(File.DirectoryName, $"{Name}.{version}.nupkg");
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);

            Version = version;
            Save();

            await RunAsync(
                "pack",
                $"dotnet pack {File.FullName} --output . -p:PackageVersion={version} -p:SymbolPackageFormat=snupkg",
                token);

            return file;
        }

        public async Task PublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token)
        {
            var packageFile = await PackAsync(version, token);

            await RunAsync(
                "publish",
                $"dotnet nuget push {packageFile} --source {new Uri(registry, Constants.ServerPathSuffix)} --api-key {accessToken}",
                token);

            System.IO.File.Delete(packageFile);
        }
    }
}