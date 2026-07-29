using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
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
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.Add(new Configuration()).AsSelf().Singleton();

        return Task.CompletedTask;
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
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

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        Migrator
            .Instance.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Project)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();

        return Task.CompletedTask;
    }
}
