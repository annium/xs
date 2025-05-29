using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Tools;

namespace Annium.Xs.Cli.Core.Projects;

public interface IPlatformProjectFactory
{
    ProjectType Type { get; }

    bool IsProjectDirectory(string directory);

    bool IsProjectFile(string file);

    IProject CreateProject(string directory, DiscoverConfiguration discoverCfg, PlatformConfigurationBase? projectCfg);
}
