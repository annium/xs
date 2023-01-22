using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using LinqToDB;
using LinqToDB.Data;
using Server.Domain.Interfaces;

namespace Server.Db.Shared.Repositories;

internal class PackageRepository<TPackage, TPackageDependency, TPackageEntity, TPackageDependencyEntity, TContext> : IPackageRepository<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
    where TPackageEntity : class, Server.Db.Shared.Entities.IPackage<TPackageDependencyEntity>, new()
    where TPackageDependencyEntity : class, Server.Db.Shared.Entities.IPackageDependency
    where TContext : IContext
{
    private readonly TContext _context;

    private readonly ITable<TPackageEntity> _packages;

    private readonly ITable<TPackageDependencyEntity> _packageDependencies;

    private readonly IMapper _mapper;

    public PackageRepository(
        TContext context,
        Func<TContext, ITable<TPackageEntity>> getPackagesTable,
        Func<TContext, ITable<TPackageDependencyEntity>> getPackageDependenciesTable,
        IMapper mapper
    )
    {
        _context = context;
        _packages = getPackagesTable(context);
        _packageDependencies = getPackageDependenciesTable(context);
        _mapper = mapper;
    }

    public async Task<TPackage> CreateAsync(TPackage package)
    {
        var entity = _mapper.Map<TPackageEntity>(package);
        entity.Id = Guid.NewGuid();
        entity.Dependencies.ForEach(d => d.PackageId = entity.Id);

        await using (var db = _context.GetDataConnection())
        {
            await db.InsertAsync(entity);
            db.BulkCopy(entity.Dependencies);
        }

        return _mapper.Map<TPackage>(entity);
    }

    public async Task<TPackage[]> FindAllByNameAsync(string name)
    {
        name = name.ToLower();

        var entities = await _packages
            .Where(p => p.LowerName == name)
            .OrderByDescending(p => p.Version)
            .ToArrayAsync();

        var ids = entities.Select(e => e.Id).ToArray();
        var dependencies = await _packageDependencies
            .Where(d => ids.Contains(d.PackageId))
            .ToArrayAsync();

        foreach (var entity in entities)
            entity.Dependencies = dependencies.Where(p => p.PackageId == entity.Id).ToList();

        return entities.Select(_mapper.Map<TPackage>).ToArray();
    }

    public Task<string[]> FindAllVersionsByNameAsync(string name)
    {
        name = name.ToLower();

        return _packages
            .Where(p => p.LowerName == name)
            .Select(p => p.Version)
            .OrderByDescending(v => v)
            .ToArrayAsync();
    }

    public async Task<TPackage> FindByNameVersionAsync(string name, string version)
    {
        name = name.ToLower();

        var entity = await _packages
            .Where(p => p.LowerName == name && p.Version == version)
            .FirstOrDefaultAsync();

        if (entity is not null)
            entity.Dependencies = await _packageDependencies
                .Where(d => d.PackageId == entity.Id)
                .ToListAsync();

        return _mapper.Map<TPackage>(entity);
    }

    public Task<int> CountAllDownloadsAsync(string name)
    {
        name = name.ToLower();

        return _packages.Where(p => p.LowerName == name).SumAsync(p => p.Downloads);
    }

    public async Task IncrementDownloadsAsync(Guid id)
    {
        var downloads = await _packages
            .Where(p => p.Id == id)
            .Select(p => p.Downloads)
            .FirstOrDefaultAsync();

        await _packages
            .Where(p => p.Id == id && p.Downloads == downloads)
            .UpdateAsync(p => new TPackageEntity { Downloads = downloads + 1 });
    }

    public Task DeleteByNameVersionAsync(string name, string version)
    {
        name = name.ToLower();

        return _packages.Where(p => p.LowerName == name && p.Version == version).DeleteAsync();
    }
}