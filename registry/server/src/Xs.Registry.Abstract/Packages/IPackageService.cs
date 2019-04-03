using System.Threading.Tasks;
using Annium.Data.Operations;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Packages
{
    public interface IPackageService<TPackage, TPackageDependency, TPayload> where TPayload : class, IPayload where TPackage : class, IPackage<TPackageDependency> where TPackageDependency : class, IPackageDependency
    {
        Task<StatusResult<PackageStatus>> PublishPackageAsync(User user, TPayload payload);

        Task<StatusResult<PackageStatus>> UnpublishPackageAsync(User user, string name, string version);

        Task<StatusResult<PackageStatus, TPackage[]>> GetPackagesAsync(User user, string name);

        Task<StatusResult<PackageStatus>> ProcessDownloadAsync(User user, string name, string version, bool countDownload);
    }
}