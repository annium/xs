using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.linq2db.Extensions;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Repositories;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;

namespace Annium.Xs.Server.Shared.Internal.Repositories;

internal class MetaPackageRepository : RepositoryBase<Connection>, IMetaPackageRepository
{
    public MetaPackageRepository(Connection db)
        : base(db) { }

    public async Task CreateAsync(MetaPackage metaPackage)
    {
        await Db.MetaPackages.InsertAsync(metaPackage);
        await Db.MetaPackagePermissions.BulkCopyAsync(metaPackage.Permissions);
    }

    public async Task<IReadOnlyCollection<MetaPackage>> FindAllAsync(
        Guid userId,
        ProjectType? type,
        string? query,
        int page,
        int count
    )
    {
        // pages are 1-based. Without this the offset goes negative and Postgres rejects the query with an
        // opaque 2201X, surfacing as a 500 instead of a clear contract violation at the call site.
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        // visible = yours, or explicitly published to the world. The World grant is what an owner sets via
        // POST /{type}/{name}/permissions; packages start at World: None (MetaPackageTool.Generate), i.e.
        // private. Matching on the Owner category instead would match nearly every row, since every package
        // is created with an Owner Read|Publish grant — mirrors MetaPackageAccess.ForUser's own resolution.
        var request = Db.MetaPackages.Where(x =>
            x.OwnerId == userId
            || x.Permissions.Any(p =>
                p.Category == PermissionCategory.World && (p.Permission & Permission.Read) == Permission.Read
            )
        );

        if (type is not null)
            request = request.Where(x => x.Type == type);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var upperQuery = query.ToUpperInvariant();
            request = request.Where(x => x.Name.ToUpper().Contains(upperQuery));
        }

        var entities = await request
            .LoadWith(x => x.Owner)
            .LoadWith(x => x.Permissions)
            .AsQueryable()
            // Skip/Take without an ORDER BY leaves row order undefined, so successive pages could overlap
            // or drop entries. Id is unique, which makes the ordering total and pagination stable.
            .OrderBy(x => x.Id)
            .Skip((page - 1) * count)
            .Take(count)
            .ToArrayAsync();

        return entities;
    }

    public async Task<MetaPackage?> TryGetByIdAsync(Guid id)
    {
        return await Db.MetaPackages.LoadWith(x => x.Permissions).AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<MetaPackageAccess?> TryGetAccessByIdAsync(Guid id)
    {
        var data = await Db
            .MetaPackages.LoadWith(x => x.Permissions)
            .AsQueryable()
            .Where(x => x.Id == id)
            .Select(x => new { owner = x.OwnerId, permissions = x.Permissions })
            .FirstOrDefaultAsync();

        return data is null ? null : new MetaPackageAccess(data.owner, data.permissions);
    }

    public async Task<MetaPackage?> TryFindByTypeNameAsync(ProjectType type, string name)
    {
        return await Db
            .MetaPackages.LoadWith(x => x.Permissions)
            .AsQueryable()
            .FirstOrDefaultAsync(x => x.Type == type && x.Name == name);
    }

    public async Task UpdateInfoAsync(Guid id, IPackageInfo info)
    {
        await Db
            .MetaPackages.Where(x => x.Id == id)
            .Set(x => x.Name, info.Name)
            .Set(x => x.Version, info.Version)
            .Set(x => x.Description, info.Description)
            .Set(x => x.Published, info.Published)
            .UpdateAsync();
    }

    public async Task SetDownloadsAsync(Guid id, int downloads)
    {
        await Db.MetaPackages.Where(x => x.Id == id).Set(x => x.Downloads, downloads).UpdateAsync();
    }

    public async Task UpdatePermissionsAsync(Guid id, IReadOnlyCollection<MetaPackagePermission> permissions)
    {
        foreach (var permission in permissions)
            await Db
                .MetaPackagePermissions.Where(p => p.MetaPackageId == id && p.Category == permission.Category)
                .Set(x => x.Permission, permission.Permission)
                .UpdateAsync();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        await Db.MetaPackages.DeleteAsync(x => x.Id == id);
    }
}
