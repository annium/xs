using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.PostgreSql;
using Annium.Xs.Server.Abstractions;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Dotnet.Domain;
using Annium.Xs.Server.Dotnet.Internal;
using Annium.Xs.Server.Dotnet.Internal.Services;
using Annium.Xs.Server.Dotnet.Services;
using Annium.Xs.Server.Dotnet.Views.Requests;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Dotnet;

public class ServicePack : PackageServicePackBase<Package, PackageDependency, PackageRequest>
{
    protected override ProjectType ProjectType => Constants.ProjectType;

    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.Add(new Configuration()).AsSelf().Singleton();

        return Task.CompletedTask;
    }

    public override async Task RegisterAsync(
        IServiceContainer container,
        IServiceProvider provider,
        CancellationToken ct
    )
    {
        // TODO: setup with index

        // auth — the shared bearer accessor is registered by Shared.ServicePack; this adds the
        // NuGet-specific header accessor on top of it
        container.Add(new HeaderTokenAccessor("X-NuGet-ApiKey")).AsInterfaces().Singleton();

        // packages
        await base.RegisterAsync(container, provider, ct);
        container.Add<ISymbolStorage, SymbolStorage>().Singleton();
    }

    protected override void RegisterPackageRequestParser(IServiceContainer container) =>
        container
            .Add<IPackageRequestParser<Package, PackageDependency, PackageRequest>, PackageRequestParser>()
            .Singleton();

    protected override void RegisterPackageStorage(IServiceContainer container) =>
        container.Add<PackageStorage>().AsInterfaces().Singleton();

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        Migrator
            .Instance.ForPostgresql(provider.Resolve<PostgreSqlConfiguration>().ConnectionString, Constants.Project)
            .WithScriptsFromAssembly(GetType().Assembly)
            .Execute();

        return Task.CompletedTask;
    }
}
