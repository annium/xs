using System;
using System.IO;
using System.Text;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools
{
    internal class SpecialConfigurationManager : ISpecialConfigurationManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        private const string file = ".npmrc";

        public void Save(IProject project, Uri location, string token)
        {
            // with NPM currently it's not possible to publish unscoped packages privately
            if (!project.Name.StartsWith('@'))
                return;

            var scope = project.Name.Substring(1).Split('/') [0];
            var sb = new StringBuilder();
            sb.AppendLine($"@{scope}:registry={location}");
            sb.AppendLine($"//{location.Authority}/:_authToken=\"{token}\"");
            File.WriteAllText(FilePath(project), sb.ToString());
        }

        public void Delete(IProject project)
        {
            var path = FilePath(project);
            if (File.Exists(path)) File.Delete(path);
        }

        private string FilePath(IProject project) => Path.Combine(project.File.DirectoryName, file);
    }
}