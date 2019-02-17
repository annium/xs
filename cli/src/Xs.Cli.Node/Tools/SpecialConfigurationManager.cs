using System;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools
{
    internal class SpecialConfigurationManager : ISpecialConfigurationManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public void Save(IProject project, Uri location, string token)
        {
            // TODO: implement
        }

        public void Delete(IProject project)
        {
            // TODO: implement
        }
    }
}