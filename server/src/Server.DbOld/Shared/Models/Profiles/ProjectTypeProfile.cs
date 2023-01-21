using Annium.Core.Mapper;

namespace Server.Db.Shared.Models.Profiles;

public class ProjectTypeProfile : Profile
{
    public ProjectTypeProfile()
    {
        Map<string, ProjectType>(x => ProjectType.Register(x));
    }
}