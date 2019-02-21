using System.Threading.Tasks;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Packages
{
    public interface IPackageService<TPackage, TPackageDependency, TPayload> where TPayload : class, IPayload where TPackage : class, IPackage<TPackageDependency> where TPackageDependency : class, IPackageDependency
    {
        Task<IPackageResult> PublishPackageAsync(User user, TPayload payload);

        Task<IPackageResult> UnpublishPackageAsync(User user, string name, string version);

        Task<IPackageResult> GetPackagesAsync(User user, string name);

        Task<IPackageResult> TrackDownloadAsync(User user, string name, string version);
    }
}