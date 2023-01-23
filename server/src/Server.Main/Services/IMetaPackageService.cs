using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Shared.Domain.Models;

namespace Server.Main.Services;

public interface IMetaPackageService
{
    Task<IReadOnlyCollection<MetaPackage>> FindAllAsync(Guid userId, ProjectType? type, string? query, int page, int count);
    Task<MetaPackage?> TryFindByTypeNameAsync(ProjectType type, string name);
    Task UpdatePermissionsAsync(Guid id, IReadOnlyCollection<MetaPackagePermission> permissions);
}