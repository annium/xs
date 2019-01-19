using System;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xs.Registry.Core;
using Xs.Registry.Node.Repositories;

namespace Xs.Registry.Node
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Core.BaseServicePack>();
            Add<Core.ServicePack>();
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(
                new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("configuration", "node.json"))
                .AddJsonFile(Path.Combine("configuration", "node.override.json"), optional : true)
                .Build<Configuration>()
            );
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // db collections
            var db = GetDatabase(
                provider.GetRequiredService<Core.Configuration>().Database,
                provider.GetRequiredService<Node.Configuration>().Database
            );
            services.AddSingleton(db.GetCollection<Repositories.Models.Package>("packages"));

            // repositories
            services.AddSingleton<IPackageRepository, PackageRepository>();
        }

        public override void Setup(IServiceProvider provider)
        {
            provider.GetRequiredService<IMongoCollection<Repositories.Models.Package>>().Indexes.CreateOne(
                new CreateIndexModel<Repositories.Models.Package>(
                    Builders<Repositories.Models.Package>.IndexKeys
                    .Ascending(nameof(Repositories.Models.Package.Name))
                    .Ascending(nameof(Repositories.Models.Package.Version))
                )
            );
        }

        private IMongoDatabase GetDatabase(
            Core.DatabaseConfiguration shared,
            Node.DatabaseConfiguration node
        ) => DatabaseAccessor.GetDatabase(shared.Host, shared.Port, node.Name, shared.User, shared.Pass, shared.LogQueries);
    }
}