using Xs.Cli.Core.Commands;

namespace Xs.Cli.Core.Projects;

public interface IProjectMapper<TProject, TRawProject>
{
    TRawProject Load(string path, DiscoverConfiguration configuration);

    void Save(TProject project);
}