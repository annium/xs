using System;
using System.Diagnostics;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db
{
    internal class BaseServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            var cfg = new ConfigurationBuilder()
                .AddYamlFile(Path.Combine("configuration", "db.yml"))
                .Build<Configuration>();

            // register context itself
            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<Context>(builder =>
                {
                    builder.UseNpgsql(
                        string.Join(';', new string[]
                        {
                            $"Host={cfg.Host}",
                            $"Port={cfg.Port}",
                            $"Database={cfg.Name}",
                            $"Username={cfg.User}",
                            $"Password={cfg.Pass}",
                            $"SSL Mode=Prefer",
                            $"Trust Server Certificate=true",
                        }),
                        options =>
                        {
                            options.UseNodaTime(); // is needed, cause not enabled by default
                        }
                    );
                });

            // init linq2db for EF Core
            LinqToDBForEFTools.Initialize();
            DataConnection.TurnTraceSwitchOn(TraceLevel.Verbose);
            DataConnection.WriteTraceLine = (message, context) => Console.WriteLine($"{context}: {message}");
            LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;
        }
    }
}