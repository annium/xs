using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Annium.linq2db.Extensions;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Internal.Repositories;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;

namespace Annium.Xs.Server.Abstractions.Internal.Db.Repositories;

internal class PackageRepository<TPackage, TPackageDependency>
    : RepositoryBase<ServerConnection<TPackage, TPackageDependency>>,
        IPackageRepository<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    public PackageRepository(ServerConnection<TPackage, TPackageDependency> db)
        : base(db) { }

    public async Task CreateAsync(TPackage package)
    {
        await Db.Packages.InsertAsync(package);
        await Db.BulkCopyAsync(package.Dependencies);
    }

    /// <summary>
    /// Case-insensitive name match. Returned as an expression so linq2db keeps translating it to SQL.
    /// </summary>
    private static Expression<Func<TPackage, bool>> ByName(string name)
    {
        var upperName = name.ToUpperInvariant();

        return x => x.Name.ToUpper() == upperName;
    }

    public async Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name)
    {
        var entities = await Db
            .Packages.Where(ByName(name))
            .LoadWith(x => x.Dependencies)
            .OrderByDescending(p => p.Version)
            .ToArrayAsync();

        return entities;
    }

    public async Task<TPackage?> TryFindByNameVersionAsync(string name, string version)
    {
        var entity = await Db
            .Packages.Where(ByName(name))
            .Where(x => x.Version == version)
            .LoadWith(x => x.Dependencies)
            .AsQueryable()
            .FirstOrDefaultAsync();

        return entity;
    }

    public async Task<int> CountAllDownloadsAsync(string name)
    {
        return await Db.Packages.Where(ByName(name)).SumAsync(p => p.Downloads);
    }

    public async Task IncrementDownloadsAsync(Guid id)
    {
        await Db.Packages.Where(x => x.Id == id).Set(x => x.Downloads, x => x.Downloads + 1).UpdateAsync();
    }

    public async Task DeleteByNameVersionAsync(string name, string version)
    {
        await Db.Packages.Where(ByName(name)).Where(x => x.Version == version).DeleteAsync();
    }
}
