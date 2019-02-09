using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Db.Shared
{
    internal class MetaPackageRepository : IMetaPackageRepository
    {
        private readonly ISharedContext context;

        private readonly IMapper mapper;

        public MetaPackageRepository(
            ISharedContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MetaPackage> CreateAsync(MetaPackage metaPackage)
        {
            var entity = mapper.Map<Entities.MetaPackage>(metaPackage);

            context.Entry(entity).State = EntityState.Added;
            foreach (var permission in entity.Permissions)
                context.Entry(permission).State = EntityState.Added;

            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;
            foreach (var permission in entity.Permissions)
                context.Entry(permission).State = EntityState.Detached;

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task<MetaPackage> GetByIdAsync(Guid id)
        {
            var entity = await context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Permissions)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task<MetaPackage[]> FindAllByOwnerIdAsync(Guid ownerId)
        {
            var entities = await context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.Permissions)
                .Where(p => p.OwnerId == ownerId)
                .ToListAsync();

            return entities.Select(mapper.Map<MetaPackage>).ToArray();
        }

        public async Task<MetaPackage[]> FindPackagesByQueryAsync(Guid userId, string query, int page, int count)
        {
            var request = context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.Permissions)
                .Where(p => p.OwnerId == userId ||
                    p.Permissions.Any(e => e.Category == PermissionCategory.World && e.Permission.HasFlag(Permission.Read))
                );

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                request = request.Where(p => p.Name.ToLower().Contains(query));
            }

            request = request
                .OrderBy(p => p.Name)
                .Skip((page - 1) * count)
                .Take(count);

            var entities = await request.ToListAsync();

            return entities.Select(mapper.Map<MetaPackage>).ToArray();
        }

        public async Task<MetaPackage> FindByTypeNameAsync(ProjectType type, string name)
        {
            var typeString = type.ToString();
            name = name.ToLower();

            var entity = await context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Permissions)
                .Where(p => p.Type == typeString && p.LowerName == name)
                .FirstOrDefaultAsync();

            return mapper.Map<MetaPackage>(entity);
        }

        public Task UpdateInfoAsync(Guid id, IPackageInfo packageInfo)
        {
            return context.MetaPackages
                .Where(p => p.Id == id)
                .UpdateAsync(u => new Entities.MetaPackage()
                {
                    Name = packageInfo.Name,
                        Version = packageInfo.Version,
                        Description = packageInfo.Description,
                        Published = packageInfo.Published,
                });
        }

        public Task SetDownloadsAsync(Guid id, int downloads)
        {
            return context.MetaPackages
                .Where(p => p.Id == id)
                .UpdateAsync(u => new Entities.MetaPackage { Downloads = downloads });
        }

        public async Task IncrementDownloadsAsync(Guid id)
        {
            var downloads = await context.MetaPackages
                .Where(p => p.Id == id)
                .Select(p => p.Downloads)
                .FirstOrDefaultAsync();

            await context.MetaPackages
                .Where(p => p.Id == id && p.Downloads == downloads)
                .UpdateAsync(p => new Entities.MetaPackage { Downloads = downloads + 1 });
        }

        public Task DeleteByIdAsync(Guid id)
        {
            return context.MetaPackages.Where(p => p.Id == id).DeleteAsync();
        }

        public Task DeleteByTypeNameAsync(ProjectType type, string name)
        {
            var typeString = type.ToString();
            name = name.ToLower();

            return context.MetaPackages.Where(p => p.Type == typeString && p.LowerName == name).DeleteAsync();
        }
    }
}