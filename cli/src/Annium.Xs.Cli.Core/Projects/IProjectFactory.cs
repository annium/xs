namespace Annium.Xs.Cli.Core.Projects;

public interface IProjectFactory
{
    IPlatformProjectFactory? ResolveFactory(string directory);

    bool IsProjectFile(string file);
}
