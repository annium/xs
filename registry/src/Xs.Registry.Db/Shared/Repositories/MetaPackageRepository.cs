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

        public async Task<MetaPackageAccess> GetAccessByIdAsync(Guid id)
        {
            var data = await context.MetaPackages
                .Where(p => p.Id == id)
                .Include(p => p.Permissions)
                .Select(p => new { owner = p.OwnerId, permissions = p.Permissions })
                .FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new MetaPackageAccess(data.owner, data.permissions.Select(mapper.Map<MetaPackagePermission>).ToArray());
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

        public async Task<MetaPackage[]> FindPackagesAsync(
            Guid userId,
            Guid ownerId,
            ProjectType type,
            string query,
            int page,
            int count
        )
        {
            var request = context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.Permissions)
                .AsQueryable();

            // if ownerId passed and is equal to userId - searching own packages, so skip access check
            if (ownerId == userId)
                request = request.Where(p => p.OwnerId == userId);

            // otherwise, if ownerId specified - search user's packages, applying access check
            else if (ownerId != Guid.Empty)
                request = request.Where(p => p.OwnerId == ownerId &&
                    p.Permissions.Any(e => e.Category == PermissionCategory.World && e.Permission.HasFlag(Permission.Read))
                );

            // otherwise, if ownerId not specified - generic search with access check
            else
                request = request.Where(p => p.OwnerId == userId ||
                    p.Permissions.Any(e => e.Category == PermissionCategory.World && e.Permission.HasFlag(Permission.Read))
                );

            if (type != null)
            {
                var typeString = type.ToString();
                request = request.Where(p => p.Type == typeString);
            }

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
                .Include(p => p.Owner)
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

        public async Task UpdatePermissionsAsync(Guid id, MetaPackagePermission[] permissions)
        {
            foreach (var permission in permissions)
                await context.MetaPackagePermissions
                .Where(p => p.MetaPackageId == id && p.Category == permission.Category)
                .UpdateAsync(p => new Entities.MetaPackagePermission { Permission = permission.Permission });
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