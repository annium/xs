using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Db.Repositories;

internal interface IMetaPackageRepository
{
    Task CreateAsync(MetaPackage metaPackage);
    Task<IReadOnlyCollection<MetaPackage>> FindAllAsync(Guid userId, Guid ownerId, ProjectType? type, string? query, int page, int count);
    Task<MetaPackage?> TryGetByIdAsync(Guid id);
    Task<MetaPackageAccess?> TryGetAccessByIdAsync(Guid id);
    Task<MetaPackage?> TryFindByTypeNameAsync(ProjectType type, string name);
    Task UpdateInfoAsync(Guid id, IPackageInfo packageInfo);
    Task SetDownloadsAsync(Guid id, int downloads);
    Task UpdatePermissionsAsync(Guid id, IReadOnlyCollection<MetaPackagePermission> permissions);
    Task DeleteByIdAsync(Guid id);
}