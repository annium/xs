using System;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Server.Shared.Internal;
using Server.Shared.Internal.Auth;
using Server.Shared.Internal.Tools;
using Server.Shared.Tools;
using Xdb.Core.Migrations;

namespace Server.Shared;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddPostgreSql<Connection>();

        // auth
        container.Add<IApplicationModelProvider, AuthorizationApplicationModelProvider>().Singleton();
        container.Add<AuthorizationFilter>().AsSelf().Singleton();

        // tools
        container.Add<IMetaPackageTool, MetaPackageTool>().Singleton();

        // repositories
        container
            .AddAll(GetType().Assembly)
            .Where(x => x.IsClass && x.Name.EndsWith("Repository"))
            .AsInterfaces()
            .Scoped();
    }

    public override void Setup(IServiceProvider provider)
    {
        Migrator
            .ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Schema)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();
    }
}
