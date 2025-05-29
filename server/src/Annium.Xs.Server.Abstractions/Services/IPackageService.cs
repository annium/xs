using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Abstractions.Services;

public interface IPackageService<TPackage, TPackageDependency, TPackageRequest>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
    where TPackageRequest : class, IPackageRequest
{
    Task<IStatusResult<PackageStatus, IReadOnlyCollection<TPackage>>> GetPackagesAsync(User user, string name);
    Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name);
    Task<TPackage?> TryFindByNameVersionAsync(string name, string version);
    Task<IStatusResult<PackageStatus>> PublishPackageAsync(User user, TPackageRequest request);
    Task<IStatusResult<PackageStatus>> UnpublishPackageAsync(User user, string name, string version);
    Task<IStatusResult<PackageStatus>> ProcessDownloadAsync(
        User? user,
        string name,
        string version,
        bool countDownload
    );
}
