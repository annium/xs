using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tools;

public interface IPlatformConfigurationManager
{
    ProjectType Type { get; }
    string[] IgnorePatterns { get; }

    void Save(IProject project, ProjectTypeConfiguration configuration);

    void Delete(IProject project);
}
