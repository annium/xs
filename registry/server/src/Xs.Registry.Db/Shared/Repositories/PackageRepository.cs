using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using LinqToDB;
using LinqToDB.Data;

namespace Xs.Registry.Db.Shared
{
    internal class PackageRepository<TPackage, TPackageDependency, TPackageEntity, TPackageDependencyEntity, TContext> : IPackageRepository<TPackage, TPackageDependency>
        where TPackage : class, IPackage<TPackageDependency>
        where TPackageDependency : class, IPackageDependency
    where TPackageEntity : class, Entities.IPackage<TPackageDependencyEntity>, new()
    where TPackageDependencyEntity : class, Entities.IPackageDependency
    where TContext : IContext
    {
        private readonly TContext context;

        private readonly ITable<TPackageEntity> packages;

        private readonly ITable<TPackageDependencyEntity> packageDependencies;

        private readonly IMapper mapper;

        public PackageRepository(
            TContext context,
            Func<TContext, ITable<TPackageEntity>> getPackagesTable,
            Func<TContext, ITable<TPackageDependencyEntity>> getPackageDependenciesTable,
            IMapper mapper
        )
        {
            this.context = context;
            packages = getPackagesTable(context);
            packageDependencies = getPackageDependenciesTable(context);
            this.mapper = mapper;
        }

        public async Task<TPackage> CreateAsync(TPackage package)
        {
            var entity = mapper.Map<TPackageEntity>(package);
            entity.Id = Guid.NewGuid();
            entity.Dependencies.ForEach(d => d.PackageId = entity.Id);

            using(var db = context.GetDataConnection())
            {
                await db.InsertAsync(entity);
                db.BulkCopy(entity.Dependencies);
            }

            return mapper.Map<TPackage>(entity);
        }

        public async Task<TPackage[]> FindAllByNameAsync(string name)
        {
            name = name.ToLower();

            var entities = await packages
                .Where(p => p.LowerName == name)
                .OrderByDescending(p => p.Version)
                .ToArrayAsync();

            var ids = entities.Select(e => e.Id).ToArray();
            var dependencies = await packageDependencies
                .Where(d => ids.Contains(d.PackageId))
                .ToArrayAsync();

            foreach (var entity in entities)
                entity.Dependencies = dependencies.Where(p => p.PackageId == entity.Id).ToList();

            return entities.Select(mapper.Map<TPackage>).ToArray();
        }

        public Task<string[]> FindAllVersionsByNameAsync(string name)
        {
            name = name.ToLower();

            return packages
                .Where(p => p.LowerName == name)
                .Select(p => p.Version)
                .OrderByDescending(v => v)
                .ToArrayAsync();
        }

        public async Task<TPackage> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            var entity = await packages
                .Where(p => p.LowerName == name && p.Version == version)
                .FirstOrDefaultAsync();

            if (entity != null)
                entity.Dependencies = await packageDependencies
                .Where(d => d.PackageId == entity.Id)
                .ToListAsync();

            return mapper.Map<TPackage>(entity);
        }

        public Task<int> CountAllDownloadsAsync(string name)
        {
            name = name.ToLower();

            return packages.Where(p => p.LowerName == name).SumAsync(p => p.Downloads);
        }

        public async Task IncrementDownloadsAsync(Guid id)
        {
            var downloads = await packages
                .Where(p => p.Id == id)
                .Select(p => p.Downloads)
                .FirstOrDefaultAsync();

            await packages
                .Where(p => p.Id == id && p.Downloads == downloads)
                .UpdateAsync(p => new TPackageEntity { Downloads = downloads + 1 });
        }

        public Task DeleteByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            return packages.Where(p => p.LowerName == name && p.Version == version).DeleteAsync();
        }
    }
}