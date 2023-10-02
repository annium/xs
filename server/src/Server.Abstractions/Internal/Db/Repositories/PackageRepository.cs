using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.linq2db.Extensions;
using LinqToDB;
using LinqToDB.Data;
using Server.Abstractions.Db.Repositories;
using Server.Shared.Domain.Interfaces;
using Server.Shared.Internal.Repositories;

namespace Server.Abstractions.Internal.Db.Repositories;

internal class PackageRepository<TPackage, TPackageDependency> :
    RepositoryBase<ServerConnection<TPackage, TPackageDependency>>,
    IPackageRepository<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency

{
    public PackageRepository(
        ServerConnection<TPackage, TPackageDependency> db
    ) : base(db)
    {
    }

    public async Task CreateAsync(TPackage package)
    {
        await Db.Packages.InsertAsync(package);
        await Db.BulkCopyAsync(package.Dependencies);
    }

    public async Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name)
    {
        var upperName = name.ToUpperInvariant();
        var entities = await Db.Packages
            .Where(x => x.Name.ToUpper() == upperName)
            .LoadWith(x => x.Dependencies)
            .OrderByDescending(p => p.Version)
            .ToArrayAsync();

        return entities;
    }

    public async Task<TPackage?> TryFindByNameVersionAsync(string name, string version)
    {
        var upperName = name.ToUpperInvariant();
        var entity = await Db.Packages
            .Where(x => x.Name.ToUpper() == upperName && x.Version == version)
            .LoadWith(x => x.Dependencies)
            .FirstOrDefaultAsync();

        return entity;
    }

    public async Task<int> CountAllDownloadsAsync(string name)
    {
        return await Db.Packages.Where(x => x.Name == name).SumAsync(p => p.Downloads);
    }

    public async Task IncrementDownloadsAsync(Guid id)
    {
        await Db.Packages
            .Where(x => x.Id == id)
            .Set(x => x.Downloads, x => x.Downloads + 1)
            .UpdateAsync();
    }

    public async Task DeleteByNameVersionAsync(string name, string version)
    {
        var upperName = name.ToUpperInvariant();
        await Db.Packages.DeleteAsync(x => x.Name.ToUpper() == upperName && x.Version == version);
    }
}