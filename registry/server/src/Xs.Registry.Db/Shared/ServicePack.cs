using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db.Shared
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddMapperConfiguration(ConfigureMapping);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddScoped<ISharedContext>(p => p.GetRequiredService<Context>());

            // repositories
            services.AddScoped<IMetaPackageRepository, MetaPackageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();

            // tools
            services.AddScoped<IMetaPackageManager, MetaPackageManager>();
        }

        private void ConfigureMapping(MapperConfiguration cfg)
        {
            cfg.Map<ProjectType, string>(t => t.ToString());
            cfg.Map<string, ProjectType>(t => ProjectType.Get(t));
            cfg.Map<MetaPackage, Entities.MetaPackage>()
                .Field(e => e.Name.ToLower(), e => e.LowerName);
            cfg.Map<MetaPackagePermission, Entities.MetaPackagePermission>()
                .Ignore(e => e.MetaPackageId);
        }
    }
}