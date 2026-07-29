using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
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
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
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

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        Migrator
            .Instance.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Schema)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();

        return Task.CompletedTask;
    }
}
