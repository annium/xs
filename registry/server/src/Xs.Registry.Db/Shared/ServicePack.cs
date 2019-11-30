using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db.Shared
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddProfile(ConfigureProfile);
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

        private void ConfigureProfile(Profile p)
        {
            p.Map<ProjectType, string>(t => t.ToString());
            p.Map<string, ProjectType>(t => ProjectType.Get(t));
            p.Map<MetaPackage, Entities.MetaPackage>()
                .Field(e => e.Name.ToLower(), e => e.LowerName);
            p.Map<MetaPackagePermission, Entities.MetaPackagePermission>()
                .Ignore(e => e.MetaPackageId);
        }
    }
}