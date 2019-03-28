using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools
{
    internal class SpecialConfigurationManager : ISpecialConfigurationManager
    {
        private const string file = ".npmrc";

        private static readonly IEnumerable<string> reservedScopes = new [] { "types" };

        public ProjectType Type { get; } = Constants.ProjectType;

        public string[] IgnorePatterns { get; } = new [] { file };

        private readonly ILogger logger;

        public SpecialConfigurationManager(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public void Save(IProject project, Uri location, string token)
        {
            logger.Trace($"Save configuration for {Constants.ProjectType} project {project}");

            // with NPM currently it's not possible to publish unscoped packages privately
            var scope = GetScope(project.Name);
            if (scope == null)
            {
                logger.Trace($"Skip configuration save for {Constants.ProjectType} project {project}: no scope defined");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"@{scope}:registry={location}");
            // add all used scopes except reserved
            foreach (var dependencyScope in project.Packages.Select(d => GetScope(d.Value.Name)).Distinct().OfType<string>())
                if (!reservedScopes.Contains(dependencyScope))
                    sb.AppendLine($"@{dependencyScope}:registry={location}");
            sb.AppendLine($"//{location.Authority}/:_authToken=\"{token}\"");
            File.WriteAllText(FilePath(project), sb.ToString());

            string GetScope(string name) => name.StartsWith('@') ? name.Substring(1).Split('/') [0] : null;
        }

        public void Delete(IProject project)
        {
            var path = FilePath(project);
            if (File.Exists(path)) File.Delete(path);
        }

        private string FilePath(IProject project) => Path.Combine(project.File.DirectoryName, file);
    }
}