using System;
using System.Threading.Tasks;

namespace Xs.Registry.Db.Shared
{
    public interface IMetaPackageRepository
    {
        Task CreateAsync(MetaPackage metaPackage);

        Task<MetaPackage> GetByIdAsync(Guid id);

        Task<MetaPackage> FindByProjectTypeNameAsync(ProjectType type, string name);
    }
}