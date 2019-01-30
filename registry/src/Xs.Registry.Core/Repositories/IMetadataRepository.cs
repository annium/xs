using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public interface IMetadataRepository
    {
        Task<Metadata[]> GetByIdsAsync(string[] ids);

        Task<Metadata> GetByIdAsync(string id);

        Task<Metadata[]> FindAllByOwnerIdAsync(string ownerId);

        Task SaveAsync(Metadata metadata);

        Task DeleteByIdAsync(string id);
    }
}