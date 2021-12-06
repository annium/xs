using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;

namespace Xs.Registry.Db.Shared;

public class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddProfile(ConfigureProfile);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<Context>().AsInterfaces().Scoped();

        // repositories
        container.Add<IMetaPackageRepository, MetaPackageRepository>().Scoped();
        container.Add<IUserRepository, UserRepository>().Scoped();
        container.Add<IUserSessionRepository, UserSessionRepository>().Scoped();

        // tools
        container.Add<IMetaPackageManager, MetaPackageManager>().Scoped();
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<ProjectType, string>(t => t.ToString());
        p.Map<string, ProjectType>(t => ProjectType.Get(t));
        p.Map<MetaPackage, Entities.MetaPackage>()
            .For(e => e.LowerName, e => e.Name.ToLower());
        p.Map<MetaPackagePermission, Entities.MetaPackagePermission>()
            .Ignore(e => e.MetaPackageId);
    }
}