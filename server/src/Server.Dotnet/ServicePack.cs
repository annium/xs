using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql;
using DbUp;
using Server.Abstractions;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal;
using Server.Dotnet.Internal.Services;
using Server.Dotnet.Services;
using Server.Dotnet.Views.Requests;
using Server.Shared.Auth.TokenAccessors;

namespace Server.Dotnet;

public class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.Add(new Configuration()).AsSelf().Singleton();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // TODO: setup with index

        // auth
        container.Add(new HeaderTokenAccessor("X-NuGet-ApiKey")).AsInterfaces().Singleton();
        container.Add(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // packages
        container.AddTools<Package, PackageDependency, PackageRequest, PackageRequestParser, PackageStorage>(
            Constants.ProjectType
        );
        container.Add<ISymbolStorage, SymbolStorage>().Singleton();
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
