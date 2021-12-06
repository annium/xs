using Annium.Core.Mapper;

namespace Xs.Cli.Core.Models.Profiles;

public class ProjectTypeProfile : Profile
{
    public ProjectTypeProfile()
    {
        Map<string, ProjectType>(x => ProjectType.Get(x));
        Map<ProjectType, string>(x => x.ToString());
    }
}