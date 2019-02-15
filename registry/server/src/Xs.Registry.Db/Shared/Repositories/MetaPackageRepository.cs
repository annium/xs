using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;

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
            entity.Id = Guid.NewGuid();
            entity.Permissions.ForEach(p => p.MetaPackageId = entity.Id);

            using(var db = context.GetDataConnection())
            {
                await db.InsertAsync(entity);
                db.BulkCopy(entity.Permissions);
            }

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task<MetaPackage> GetByIdAsync(Guid id)
        {
            var entity = await context.MetaPackages
                .ToLinqToDBTable()
                .LoadWith(p => p.Permissions)
                .FirstOrDefaultAsync(p => p.Id == id);

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task<MetaPackageAccess> GetAccessByIdAsync(Guid id)
        {
            var data = await context.MetaPackages
                .ToLinqToDBTable()
                .LoadWith(p => p.Permissions)
                .Where(p => p.Id == id)
                .Select(p => new { owner = p.OwnerId, permissions = p.Permissions })
                .FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new MetaPackageAccess(data.owner, data.permissions.Select(mapper.Map<MetaPackagePermission>).ToArray());
        }

        public async Task<MetaPackage[]> FindAsync(
            Guid userId,
            Guid ownerId,
            ProjectType type,
            string query,
            int page,
            int count
        )
        {
            var request = context.MetaPackages
                .ToLinqToDBTable()
                .InnerJoin(
                    context.Users.ToLinqToDBTable(),
                    (m, u) => m.OwnerId == u.Id,
                    (m, u) => new { m, u }
                )
                .InnerJoin(
                    context.MetaPackagePermissions.ToLinqToDBTable(),
                    (mu, p) => mu.m.Id == p.MetaPackageId,
                    (mu, p) => new { m = mu.m, u = mu.u, p }
                )
                .AsQueryable();

            // if ownerId passed and is equal to userId - searching own packages, so skip access check
            if (ownerId == userId)
                request = request.Where(o => o.u.Id == userId);

            // otherwise, if ownerId specified - search user's packages, applying access check
            else if (ownerId != Guid.Empty)
                request = request.Where(
                    o => o.u.Id == ownerId &&
                    o.p.Category == PermissionCategory.World &&
                    (o.p.Permission & Permission.Read) == Permission.Read
                );

            // otherwise, if ownerId not specified - generic search with access check
            else
                request = request.Where(
                    o => o.u.Id == userId ||
                    (o.p.Category == PermissionCategory.World &&
                        (o.p.Permission & Permission.Read) == Permission.Read)
                );

            if (type != null)
            {
                var typeString = type.ToString();
                request = request.Where(o => o.m.Type == typeString);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                request = request.Where(o => o.m.LowerName.Contains(query));
            }

            var ids = await request
                .OrderBy(o => o.m.Name)
                .Select(o => o.m.Id)
                .Distinct()
                .Skip((page - 1) * count)
                .Take(count)
                .ToArrayAsync();

            var entities = await context.MetaPackages.ToLinqToDBTable()
                .LoadWith(m => m.Owner)
                .Where(m => ids.Contains(m.Id))
                .ToArrayAsync();

            var permissions = await context.MetaPackagePermissions.ToLinqToDBTable()
                .Where(p => ids.Contains(p.MetaPackageId))
                .ToArrayAsync();

            foreach (var entity in entities)
                entity.Permissions = permissions.Where(p => p.MetaPackageId == entity.Id).ToList();

            return entities.Select(mapper.Map<MetaPackage>).ToArray();
        }

        public async Task<MetaPackage> FindByTypeNameAsync(ProjectType type, string name)
        {
            var typeString = type.ToString();
            name = name.ToLower();

            var entity = await context.MetaPackages
                .ToLinqToDBTable()
                .LoadWith(p => p.Owner)
                .LoadWith(p => p.Permissions)
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
    }
}