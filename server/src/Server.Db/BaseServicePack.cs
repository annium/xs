using System;
using System.Diagnostics;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db;

public class BaseServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddConfiguration<Configuration>(x => x.AddYamlFile(Path.Combine("configuration", "db.yml")));
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register context itself
        container.Collection
            .AddEntityFrameworkNpgsql()
            .AddDbContext<Context>(builder =>
            {
                var cfg = provider.Resolve<Configuration>();
                builder.UseNpgsql(string.Join(';',
                    $"Host={cfg.Host}",
                    $"Port={cfg.Port}",
                    $"Database={cfg.Name}",
                    $"Username={cfg.User}",
                    $"Password={cfg.Pass}",
                    "SSL Mode=Prefer",
                    "Trust Server Certificate=true"
                ));
            });

        // init linq2db for EF Core
        LinqToDBForEFTools.Initialize();
        DataConnection.TurnTraceSwitchOn(TraceLevel.Verbose);
        DataConnection.WriteTraceLine = (message, context, lvl) => Console.WriteLine($"{lvl} {context}: {message}");
    }
}