using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Db.Repositories;

public interface IMetaPackageRepository
{
    Task CreateAsync(MetaPackage metaPackage);

    Task<MetaPackage> GetByIdAsync(Guid id);

    Task<MetaPackageAccess> GetAccessByIdAsync(Guid id);

    Task<IReadOnlyCollection<MetaPackage>> FindAsync(
        Guid userId,
        Guid ownerId,
        ProjectType type,
        string query,
        int page,
        int count
    );

    Task<MetaPackage> FindByTypeNameAsync(ProjectType type, string name);

    Task UpdateInfoAsync(Guid id, IPackageInfo packageInfo);

    Task SetDownloadsAsync(Guid id, int downloads);

    Task UpdatePermissionsAsync(Guid id, MetaPackagePermission[] permissions);

    Task DeleteByIdAsync(Guid id);
}