using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Projects;

public interface ISpecialProjectFactory
{
    ProjectType Type { get; }

    bool IsProjectDirectory(string directory);

    bool IsProjectFile(string file);

    IProject CreateProject(string directory, DiscoverConfiguration discoverCfg, SpecialConfiguration? projectCfg);
}
