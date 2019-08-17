using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main
{
    public class TestServicePack : ServicePackBase
    {
        public TestServicePack()
        {
            Add<BaseServicePack>();
            Add<Db.TestBaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            var rawConfiguration = new RawConfiguration();
            rawConfiguration.Servers = new Dictionary<string, Uri>();
            rawConfiguration.Servers["dotnet"] = new Uri("http://localhost:9902");
            rawConfiguration.Servers["node"] = new Uri("http://localhost:9903");

            foreach (var type in rawConfiguration.Servers.Keys)
                ProjectType.Register(type);
            var configuration = Mapper.Map<Configuration>(rawConfiguration);

            services.AddSingleton(configuration);
        }
    }
}