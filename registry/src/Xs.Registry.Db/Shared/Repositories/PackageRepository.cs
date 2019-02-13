using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;

namespace Xs.Registry.Db.Shared
{
    internal class PackageRepository<TPackage, TPackageEntity, TPackageDependencyEntity, TContext> : IPackageRepository<TPackage>
        where TPackage : class, IPackage
    where TPackageEntity : class, Entities.IPackage<TPackageDependencyEntity>, new()
    where TPackageDependencyEntity : class, Entities.IPackageDependency
    where TContext : IContext
    {
        private readonly TContext context;

        private readonly Microsoft.EntityFrameworkCore.DbSet<TPackageEntity> packages;

        private readonly IMapper mapper;

        public PackageRepository(
            TContext context,
            Func<TContext, Microsoft.EntityFrameworkCore.DbSet<TPackageEntity>> getPackagesSet,
            IMapper mapper
        )
        {
            this.context = context;
            this.packages = getPackagesSet(context);
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
                .ToLinqToDBTable()
                .Where(p => p.LowerName == name)
                .OrderByDescending(p => p.Version)
                .ToArrayAsync();

            return entities.Select(mapper.Map<TPackage>).ToArray();
        }

        public Task<string[]> FindAllVersionsByNameAsync(string name)
        {
            name = name.ToLower();

            return packages
                .ToLinqToDBTable()
                .Where(p => p.LowerName == name)
                .Select(p => p.Version)
                .OrderByDescending(v => v)
                .ToArrayAsync();
        }

        public async Task<TPackage> FindLatestByNameAsync(string name)
        {
            name = name.ToLower();

            var entity = await packages
                .ToLinqToDBTable()
                .LoadWith(p => p.Dependencies)
                .Where(p => p.LowerName == name)
                .OrderByDescending(p => p.Version)
                .FirstOrDefaultAsync();

            return mapper.Map<TPackage>(entity);
        }

        public async Task<TPackage> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            var entity = await packages
                .ToLinqToDBTable()
                .LoadWith(p => p.Dependencies)
                .Where(p => p.LowerName == name && p.Version == version)
                .FirstOrDefaultAsync();

            return mapper.Map<TPackage>(entity);
        }

        public Task<int> CountAllDownloadsAsync(string name)
        {
            name = name.ToLower();

            return packages.Where(p => p.LowerName == name).CountAsync();
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

        public Task DeleteByIdAsync(Guid id)
        {
            return packages.Where(p => p.Id == id).DeleteAsync();
        }

        public Task DeleteByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            return packages.Where(p => p.LowerName == name && p.Version == version).DeleteAsync();
        }
    }
}