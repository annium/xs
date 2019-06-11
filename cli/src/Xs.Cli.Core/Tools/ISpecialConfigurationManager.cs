using System;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tools
{
    public interface ISpecialConfigurationManager
    {
        ProjectType Type { get; }

        string[] IgnorePatterns { get; }

        void Save(IProject project, Uri location, Configuration configuration);
        
        void Delete(IProject project);
    }
}