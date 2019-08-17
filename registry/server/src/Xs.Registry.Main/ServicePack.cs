using System;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Auth;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Main
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Db.Shared.ServicePack>();
            Add<Shared.ServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            var rawConfiguration = new ConfigurationBuilder()
                .AddYamlFile(Path.Combine("configuration", "main.yml"))
                .Build<RawConfiguration>();
            foreach (var type in rawConfiguration.Servers.Keys)
                ProjectType.Register(type);
            var configuration = Mapper.Map<Configuration>(rawConfiguration);

            services.AddSingleton(configuration);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp, access));
            services.AddScoped<ISessionManager, SessionManager>();
            services.AddSingleton<ITokenAccessor>(new BearerTokenAccessor());

            // tools
            services.AddSingleton<ISecurityManager, SecurityManager>();

            // mapping
            services.AddMapper(provider);
        }
    }
}