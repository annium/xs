using System;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xs.Registry.Core.Repositories;

namespace Xs.Registry.Core
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(
                new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("configuration", "core.json"))
                .AddJsonFile(Path.Combine("configuration", "core.override.json"), optional : true)
                .Build<Configuration>()
            );
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // db
            var db = GetDatabase(provider.GetRequiredService<Configuration>().Database);
            services.AddSingleton<IMongoDatabase>(db);

            // db collections
            services.AddSingleton(db.GetCollection<Repositories.Models.Metadata>("metadata"));
            services.AddSingleton(db.GetCollection<Repositories.Models.User>("users"));

            // repositories
            services.AddSingleton<IMetadataRepository, MetadataRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
        }

        public override void Setup(IServiceProvider provider)
        {
            provider.GetRequiredService<IMongoCollection<Repositories.Models.User>>().Indexes.CreateOne(
                new CreateIndexModel<Repositories.Models.User>(
                    Builders<Repositories.Models.User>.IndexKeys
                    .Ascending(nameof(Repositories.Models.User.Name))
                )
            );
            provider.GetRequiredService<IMongoCollection<Repositories.Models.User>>().Indexes.CreateOne(
                new CreateIndexModel<Repositories.Models.User>(
                    Builders<Repositories.Models.User>.IndexKeys
                    .Ascending(nameof(Repositories.Models.User.ApiToken))
                )
            );
        }

        private IMongoDatabase GetDatabase(DatabaseConfiguration shared) =>
            DatabaseAccessor.GetDatabase(shared.Host, shared.Port, shared.Name, shared.User, shared.Pass, shared.LogQueries);
    }
}