using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public void Save(IProject project, Uri location, string token)
        {
            // with NPM currently it's not possible to publish unscoped packages privately
            var scope = GetScope(project.Name);
            if (scope == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine($"@{scope}:registry={location}");
            // add all used scopes except reserved
            foreach (var dependencyScope in project.PackageDependencies.Select(d => GetScope(d.Name)).OfType<string>())
                if (!reservedScopes.Contains(dependencyScope))
                    sb.AppendLine($"@{dependencyScope}:registry={location}");
            sb.AppendLine($"//{location.Authority}/:_authToken=\"{token}\"");
            File.WriteAllText(FilePath(project), sb.ToString());

            string GetScope(string name) => name.StartsWith('@') ? null : name.Substring(1).Split('/') [0];
        }

        public void Delete(IProject project)
        {
            var path = FilePath(project);
            if (File.Exists(path)) File.Delete(path);
        }

        private string FilePath(IProject project) => Path.Combine(project.File.DirectoryName, file);
    }
}