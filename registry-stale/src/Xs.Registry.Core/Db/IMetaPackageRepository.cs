using System;
using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Db
{
    public interface IMetaPackageRepository
    {
        // Task<MetaPackage[]> GetByIdsAsync(string[] ids);

        Task<MetaPackage> GetByIdAsync(Guid id);

        // Task<MetaPackage[]> FindAllByOwnerIdAsync(string ownerId);

        Task UpdateAsync(MetaPackage metaPackage);

        Task DeleteByIdAsync(Guid id);
    }
}