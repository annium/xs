using Xx.Cli.Core.Commands;

namespace Xx.Cli.Core.Projects;

public interface IProjectMapper<TProject, TRawProject>
{
    TRawProject Load(string path, DiscoverConfiguration configuration);

    void Save(TProject project);
}
