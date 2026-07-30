using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.PostgreSql;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
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

        // the bearer accessor is stateless and takes no configuration, so it is registered once here
        // rather than re-constructed by each ecosystem's ServicePack. Ecosystems still add their own
        // extra accessors (e.g. Dotnet's header-based NuGet key) on top of this one.
        container.Add<ITokenAccessor, BearerTokenAccessor>().Singleton();

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
