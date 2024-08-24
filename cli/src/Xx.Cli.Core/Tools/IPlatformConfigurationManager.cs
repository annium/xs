using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;

namespace Xx.Cli.Core.Tools;

public interface IPlatformConfigurationManager
{
    ProjectType Type { get; }
    string[] IgnorePatterns { get; }

    void Save(IProject project, ProjectTypeConfiguration configuration);

    void Delete(IProject project);
}
