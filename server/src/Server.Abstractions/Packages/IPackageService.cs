using System.Threading.Tasks;
using Annium.Data.Operations;
using Server.Domain.Models;

namespace Server.Abstractions.Packages;

public interface IPackageService<TPackage, TPackageDependency, TPayload> where TPayload : class, IPayload where TPackage : class, IPackage<TPackageDependency> where TPackageDependency : class, IPackageDependency
{
    Task<IStatusResult<PackageStatus>> PublishPackageAsync(User user, TPayload payload);

    Task<IStatusResult<PackageStatus>> UnpublishPackageAsync(User user, string name, string version);

    Task<IStatusResult<PackageStatus, TPackage[]>> GetPackagesAsync(User user, string name);

    Task<IStatusResult<PackageStatus>> ProcessDownloadAsync(User user, string name, string version, bool countDownload);
}