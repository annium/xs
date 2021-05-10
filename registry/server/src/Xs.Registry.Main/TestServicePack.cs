using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Xs.Registry.Db;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main
{
    public class TestServicePack : ServicePackBase
    {
        public TestServicePack()
        {
            Add<BaseServicePack>();
            Add<TestBaseServicePack>();
        }

        public override void Configure(IServiceContainer container)
        {
            var serversCfg = new[]
            {
                ("dotnet", "http://localhost:9902"),
                ("node", "http://localhost:9903")
            };

            var servers = new Dictionary<ProjectType, Uri>();
            foreach (var (type, location) in serversCfg)
                servers[ProjectType.Register(type)] = new Uri(location);
            var configuration = new Configuration { Servers = servers };

            container.Add(configuration).AsSelf().Singleton();
        }
    }
}