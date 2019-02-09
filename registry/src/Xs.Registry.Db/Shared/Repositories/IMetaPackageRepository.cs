using System;
using System.Threading.Tasks;

namespace Xs.Registry.Db.Shared
{
    public interface IMetaPackageRepository
    {
        Task<MetaPackage> CreateAsync(MetaPackage metaPackage);

        Task<MetaPackage> GetByIdAsync(Guid id);

        Task<MetaPackage[]> FindAllByOwnerIdAsync(Guid ownerId);

        Task<MetaPackage[]> FindPackagesByQueryAsync(Guid userId, string query, int page, int count);

        Task<MetaPackage> FindByTypeNameAsync(ProjectType type, string name);

        Task UpdateInfoAsync(Guid id, IPackageInfo packageInfo);

        Task SetDownloadsAsync(Guid id, int downloads);

        Task IncrementDownloadsAsync(Guid id);

        Task DeleteByIdAsync(Guid id);

        Task DeleteByTypeNameAsync(ProjectType type, string name);
    }
}