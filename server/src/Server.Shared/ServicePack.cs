using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql;
using DbUp;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Server.Shared.Internal;
using Server.Shared.Internal.Auth;
using Server.Shared.Internal.Tools;
using Server.Shared.Tools;

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
        var result = DeployChanges
            .To.PostgresqlDatabase(provider.Resolve<PostgreSqlConfiguration>().ConnectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), x => x.Contains(".Migrations."))
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade();
        if (!result.Successful)
            throw new ApplicationException($"{result.ErrorScript}: {result.Error}");
    }
}
