using System.Threading.Tasks;
using Xs.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public interface IMetadataRepository
    {
        Task<Metadata> FindByProjectTypePackageNameAsync(ProjectType projectType, string packageName);

        Task SaveAsync(Metadata metadata);

        Task DeleteByProjectTypePackageNameAsync(ProjectType projectType, string packageName);
    }
}