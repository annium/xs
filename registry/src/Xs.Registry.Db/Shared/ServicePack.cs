using System;
using Annium.Extensions.DependencyInjection;
using AutoMapper.Configuration;
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
            services.AddSingleton<MapperConfigurationExpression>(ConfigureMapping());
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ISharedContext>(p => p.GetRequiredService<Context>());

            services.AddSingleton<IMetaPackageRepository, MetaPackageRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IUserSessionRepository, UserSessionRepository>();
        }

        private MapperConfigurationExpression ConfigureMapping()
        {
            var cfg = new MapperConfigurationExpression();

            cfg.CreateMap<MetaPackage, Entities.MetaPackage>().ReverseMap();
            cfg.CreateMap<MetaPackagePermission, Entities.MetaPackagePermission>()
                .ForMember(p => p.MetaPackageId, opt => opt.Ignore())
                .ReverseMap();
            cfg.CreateMap<ProjectType, string>().ConvertUsing(t => t.ToString());
            cfg.CreateMap<string, ProjectType>().ConvertUsing(t => ProjectType.Get(t));
            cfg.CreateMap<User, Entities.User>().ReverseMap();
            cfg.CreateMap<UserSession, Entities.UserSession>().ReverseMap();

            return cfg;
        }
    }
}