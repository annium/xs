using System;
using System.Threading.Tasks;

namespace Xs.Registry.Db.Shared
{
    public interface IMetaPackageRepository
    {
        Task<MetaPackage> CreateAsync(MetaPackage metaPackage);

        Task<MetaPackage> GetByIdAsync(Guid id);

        Task<MetaPackageAccess> GetAccessByIdAsync(Guid id);

        Task<MetaPackage[]> FindAsync(
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
}