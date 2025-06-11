using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.DependencyInjection.Packs;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.PostgreSql;
using Annium.Xs.Server.Abstractions;
using Annium.Xs.Server.Dotnet.Domain;
using Annium.Xs.Server.Dotnet.Internal;
using Annium.Xs.Server.Dotnet.Internal.Services;
using Annium.Xs.Server.Dotnet.Services;
using Annium.Xs.Server.Dotnet.Views.Requests;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;

namespace Annium.Xs.Server.Dotnet;

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
        Migrator
            .Instance.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Project)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();
    }
}
