using Annium.Core.Mapper;

namespace Server.Domain.Models.Profiles;

public sealed class ProjectTypeProfile : Profile
{
    public ProjectTypeProfile()
    {
        Map<string, ProjectType>(x => ProjectType.Register(x));
    }
}