using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.linq2db.PostgreSql;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Abstractions.Internal.Db;
using Annium.Xs.Server.Abstractions.Internal.Db.Repositories;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Annium.Xs.Server.Abstractions.Internal.Tools;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Abstractions.Tools;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Abstractions;

/// <summary>
/// Base service pack carrying the multi-step registration shared by every package-tools ecosystem (db, services, tools).
/// The request parser and storage implementations are registered by <see cref="RegisterPackageRequestParser"/> and
/// <see cref="RegisterPackageStorage"/> because their concrete types are internal to each ecosystem's own assembly and
/// therefore cannot appear as generic arguments on this public base class.
/// </summary>
public abstract class PackageServicePackBase<TPackage, TPackageDependency, TPackageRequest> : ServicePackBase
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
    where TPackageRequest : class, IPackageRequest
{
    /// <summary>
    /// Project type this ecosystem's package tools are registered for
    /// </summary>
    protected abstract ProjectType ProjectType { get; }

    /// <summary>
    /// Registers the shared package repository, services and tools for this ecosystem
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <param name="provider">The service provider for resolving dependencies</param>
    /// <param name="ct">Cancellation token observed cooperatively by async pack authors</param>
    /// <returns>A task that completes when registration is done</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // db
        container.AddPostgreSql<ServerConnection<TPackage, TPackageDependency>>();
        container
            .Add<IPackageRepository<TPackage, TPackageDependency>, PackageRepository<TPackage, TPackageDependency>>()
            .Scoped();

        // services
        container
            .Add<
                IPackageService<TPackage, TPackageDependency, TPackageRequest>,
                PackageService<TPackage, TPackageDependency, TPackageRequest>
            >()
            .Scoped();
        RegisterPackageRequestParser(container);
        RegisterPackageStorage(container);

        // tools
        container
            .Add<UrlTool>(
                static (sp, projectType) =>
                {
                    var config = sp.Resolve<Shared.Configuration>();
                    var uri = config.Servers[projectType.CastTo<ProjectType>()];
                    return new UrlTool(uri);
                }
            )
            .AsKeyed<IUrlTool>(ProjectType)
            .Singleton();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers the ecosystem-specific <see cref="IPackageRequestParser{TPackage,TPackageDependency,TPackageRequest}"/> implementation as a singleton
    /// </summary>
    /// <param name="container">The service container to register the request parser in</param>
    protected abstract void RegisterPackageRequestParser(IServiceContainer container);

    /// <summary>
    /// Registers the ecosystem-specific package storage implementation, by its interfaces, as a singleton
    /// </summary>
    /// <param name="container">The service container to register the package storage in</param>
    protected abstract void RegisterPackageStorage(IServiceContainer container);
}
