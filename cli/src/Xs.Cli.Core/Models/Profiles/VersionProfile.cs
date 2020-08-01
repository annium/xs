using Annium.Core.Mapper;

namespace Xs.Cli.Core.Models.Profiles
{
    public class VersionProfile : Profile
    {
        public VersionProfile()
        {
            Map<string, Version>(x => Version.Parse(x));
            Map<Version, string>(x => x.ToString());
        }
    }
}