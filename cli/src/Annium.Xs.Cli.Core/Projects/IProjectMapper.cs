using Annium.Xs.Cli.Core.Commands;

namespace Annium.Xs.Cli.Core.Projects;

public interface IProjectMapper<TProject, TRawProject>
{
    TRawProject Load(string path, DiscoverConfiguration configuration);

    void Save(TProject project);
}
