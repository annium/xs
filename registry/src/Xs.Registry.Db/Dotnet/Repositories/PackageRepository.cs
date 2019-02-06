using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Db.Dotnet
{
    internal class PackageRepository : IPackageRepository
    {
        private readonly IDotnetContext context;

        private readonly IMapper mapper;

        public PackageRepository(
            IDotnetContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<Package> CreateAsync(Package package)
        {
            var entity = mapper.Map<Entities.Package>(package);

            context.Entry(entity).State = EntityState.Added;
            foreach (var dependency in entity.Dependencies)
                context.Entry(dependency).State = EntityState.Added;

            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;
            foreach (var dependency in entity.Dependencies)
                context.Entry(dependency).State = EntityState.Detached;

            return mapper.Map<Package>(entity);
        }

        public async Task<Package[]> FindAllByNameAsync(string name)
        {
            name = name.ToLower();

            var entities = await context.Packages
                .AsNoTracking()
                .Where(p => p.LowerName == name)
                .ToArrayAsync();

            return entities.Select(mapper.Map<Package>).ToArray();
        }

        public Task<string[]> FindAllVersionsByNameAsync(string name)
        {
            name = name.ToLower();

            return context.Packages
                .AsNoTracking()
                .Where(p => p.LowerName == name)
                .Select(p => p.Version)
                .ToArrayAsync();
        }

        public async Task<Package> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            var entity = await context.Packages
                .AsNoTracking()
                .Include(p => p.Dependencies)
                .Where(p => p.LowerName == name && p.Version == version)
                .FirstOrDefaultAsync();

            return mapper.Map<Package>(entity);
        }

        public Task<int> CountAllDownloadsAsync(string name)
        {
            name = name.ToLower();

            return context.Packages.Where(p => p.LowerName == name).CountAsync();
        }

        public async Task IncrementDownloadsAsync(Guid id)
        {
            var downloads = await context.Packages
                .Where(p => p.Id == id)
                .Select(p => p.Downloads)
                .FirstOrDefaultAsync();

            await context.Packages
                .Where(p => p.Id == id && p.Downloads == downloads)
                .UpdateAsync(p => new Entities.Package { Downloads = downloads + 1 });
        }

        public Task DeleteByNameVersionAsync(string name, string version)
        {
            name = name.ToLower();

            return context.Packages.Where(p => p.LowerName == name && p.Version == version).DeleteAsync();
        }
    }
}