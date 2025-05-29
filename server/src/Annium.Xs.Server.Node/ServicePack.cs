using System;
using Annium.Core.DependencyInjection;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.PostgreSql;
using Annium.Xs.Server.Abstractions;
using Annium.Xs.Server.Node.Domain;
using Annium.Xs.Server.Node.Internal;
using Annium.Xs.Server.Node.Internal.Services;
using Annium.Xs.Server.Node.Views.Requests;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;

namespace Annium.Xs.Server.Node;

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
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsSelf().Singleton();

        // packages
        container.AddTools<Package, PackageDependency, PackageRequest, PackageRequestParser, PackageStorage>(
            Constants.ProjectType
        );
    }

    public override void Setup(IServiceProvider provider)
    {
        Migrator
            .Instance.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Project)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();
    }
}
