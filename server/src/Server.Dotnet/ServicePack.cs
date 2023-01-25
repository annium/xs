using System;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql;
using Server.Abstractions;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal;
using Server.Dotnet.Internal.Services;
using Server.Dotnet.Services;
using Server.Dotnet.Views.Requests;
using Server.Shared.Auth.TokenAccessors;
using Xdb.Core.Migrations;

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
        container.AddTools<Package, PackageDependency, PackageRequest, PackageRequestParser, PackageStorage>(Constants.ProjectType);
        container.Add<ISymbolStorage, SymbolStorage>().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        Migrator.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Project)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();
    }
}