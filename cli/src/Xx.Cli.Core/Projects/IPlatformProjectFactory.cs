using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Tools;

namespace Xx.Cli.Core.Projects;

public interface IPlatformProjectFactory
{
    ProjectType Type { get; }

    bool IsProjectDirectory(string directory);

    bool IsProjectFile(string file);

    IProject CreateProject(string directory, DiscoverConfiguration discoverCfg, PlatformConfigurationBase? projectCfg);
}
