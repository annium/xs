using Annium.Core.Mapper;

namespace Xs.Registry.Db.Shared.Profiles
{
    public class ProjectTypeProfile : Profile
    {
        public ProjectTypeProfile()
        {
            Map<string, ProjectType>(x => ProjectType.Register(x));
        }
    }
}