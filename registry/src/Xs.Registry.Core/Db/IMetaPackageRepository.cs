using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Db
{
    public interface IMetaPackageRepository
    {
        Task<MetaPackage[]> GetByIdsAsync(string[] ids);

        Task<MetaPackage> GetByIdAsync(string id);

        Task<MetaPackage[]> FindAllByOwnerIdAsync(string ownerId);

        Task SaveAsync(MetaPackage metaPackage);

        Task DeleteByIdAsync(string id);
    }
}