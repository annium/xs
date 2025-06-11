using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.DependencyInjection.Packs;
using Annium.Core.Runtime;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.PostgreSql;
using Annium.Xs.Server.Shared.Internal;
using Annium.Xs.Server.Shared.Internal.Auth;
using Annium.Xs.Server.Shared.Internal.Tools;
using Annium.Xs.Server.Shared.Tools;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Annium.Xs.Server.Shared;

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
            .Instance.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Schema)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();
    }
}
