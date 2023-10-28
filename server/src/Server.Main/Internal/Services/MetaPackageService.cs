using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Main.Services;
using Server.Shared.Domain.Models;
using Server.Shared.Repositories;

namespace Server.Main.Internal.Services;

internal class MetaPackageService : IMetaPackageService
{
    private readonly IMetaPackageRepository _repository;

    public MetaPackageService(IMetaPackageRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyCollection<MetaPackage>> FindAllAsync(
        Guid userId,
        ProjectType? type,
        string? query,
        int page,
        int count
    ) => _repository.FindAllAsync(userId, type, query, page, count);

    public Task<MetaPackage?> TryFindByTypeNameAsync(ProjectType type, string name) =>
        _repository.TryFindByTypeNameAsync(type, name);

    public Task UpdatePermissionsAsync(Guid id, IReadOnlyCollection<MetaPackagePermission> permissions) =>
        _repository.UpdatePermissionsAsync(id, permissions);
}
