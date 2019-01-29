using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public interface IMetadataRepository
    {
        Task<Metadata> GetByIdAsync(string id);

        Task SaveAsync(Metadata metadata);

        Task DeleteByIdAsync(string id);
    }
}