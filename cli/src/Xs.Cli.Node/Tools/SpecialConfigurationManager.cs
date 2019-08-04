using System;
using System.IO;
using System.Linq;
using System.Text;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools
{
    internal class SpecialConfigurationManager : ISpecialConfigurationManager
    {
        private const string file = ".npmrc";
        public ProjectType Type { get; } = Constants.ProjectType;
        public string[] IgnorePatterns { get; } = new [] { file };
        private readonly ILogger<SpecialConfigurationManager> logger;

        public SpecialConfigurationManager(
            ILogger<SpecialConfigurationManager> logger
        )
        {
            this.logger = logger;
        }

        public void Save(IProject project, ProjectTypeConfiguration configuration)
        {
            logger.Trace($"Save configuration for {Constants.ProjectType} project {project}");

            // with NPM currently it's not possible to publish unscoped packages privately
            var scope = GetScope(project.Name);
            if (scope == null)
            {
                logger.Trace($"Skip configuration save for {Constants.ProjectType} project {project}: no scope defined");
                return;
            }

            var specialConfiguration = (SpecialConfiguration) configuration.Special;

            var sb = new StringBuilder();
            sb.AppendLine($"@{scope}:registry={configuration.Server}");
            // add all private scopes
            if (specialConfiguration != null)
                foreach (var privateScope in specialConfiguration.PrivateScopes.ToHashSet())
                    sb.AppendLine($"@{privateScope}:registry={configuration.Server}");
            sb.AppendLine($"//{configuration.Server.Authority}/:_authToken=\"{configuration.Token}\"");
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