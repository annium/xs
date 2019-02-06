using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Db.Shared
{
    internal class PackageRepository<TPackage, TPackageEntity, TPackageDependencyEntity, TContext>
        : IPackageRepository<TPackage>
        where TPackage : class, IPackage
    where TPackageEntity : class, Entities.IPackage<TPackageDependencyEntity>, new()
    where TPackageDependencyEntity : class
    where TContext : IContext
    {
        private readonly TContext context;

        private readonly DbSet<TPackageEntity> packages;

        private readonly IMapper mapper;

        public PackageRepository(
            TContext context,
            Func<TContext, DbSet<TPackageEntity>> getPackagesSet,
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

            context.Entry(entity).State = EntityState.Added;
            foreach (var dependency in entity.Dependencies)
                context.Entry(dependency).State = EntityState.Added;

            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;
            foreach (var dependency in entity.Dependencies)
                context.Entry(dependency).State = EntityState.Detached;

            return mapper.Map<TPackage>(entity);
        }

        public async Task<TPackage[]> FindAllByNameAsync(string name)
        {
            name = name.ToLower();

            var entities = await packages
                .AsNoTracking()
                .Where(p => p.LowerName == name)
                .ToArrayAsync();

            return entities.Select(mapper.Map<TPackage>).ToArray();
        }

        public Task<string[]> FindAllVersionsByNameAsync(string name)
        {
            name = name.ToLower();

            return packages
                .AsNoTracking()
                .Where(p => p.LowerName == name)
                .Select(p => p.Version)
                .ToArrayAsync();
        }

        public async Task<TPackage> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            var entity = await packages
                .AsNoTracking()
                .Include(p => p.Dependencies)
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

        public Task DeleteByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            return packages.Where(p => p.LowerName == name && p.Version == version).DeleteAsync();
        }
    }
}