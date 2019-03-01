using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
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
            TargetFramework targetFramework,
            OutputType outputType,
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
            targetFramework,
            outputType,
            auditRules,
            mapper,
            shell,
            loggerConfiguration,
            logger
        ) { }

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