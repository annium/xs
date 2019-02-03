using System;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Db
{
    internal class BaseServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            var cfg = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("configuration", "db.json"))
                .AddJsonFile(Path.Combine("configuration", "db.override.json"), optional : true)
                .Build<Configuration>();

            // register context itself
            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<Context>(builder =>
                {
                    builder.UseNpgsql(string.Join(';', new string[]
                    {
                        $"Host={cfg.Host}",
                        $"Port={cfg.Port}",
                        $"Database={cfg.Name}",
                        $"Username={cfg.User}",
                        $"Password={cfg.Password}",
                    }), options => options.UseNodaTime()); // is needed, cause not enabled by default
                    builder.EnableSensitiveDataLogging(); // TODO: remove, used for debugging only
                });

            // this one is confusing: EF+ logging configuration is separate, EF itself uses IWebHostBuilder approach
            if (cfg.LogQueries)
            {
                Action<System.Data.Common.DbCommand> executing = command => Console.WriteLine(
                    $"{command.CommandType} batch command executing: {command.CommandText}"
                );
                BatchDeleteManager.BatchDeleteBuilder = builder => builder.Executing = executing;
                BatchUpdateManager.BatchUpdateBuilder = builder => builder.Executing = executing;
            }
        }
    }
}