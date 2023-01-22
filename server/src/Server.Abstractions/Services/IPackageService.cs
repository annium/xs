using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Server.Abstractions.Domain;
using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Abstractions.Services;

public interface IPackageService<TPackage, TPackageDependency, TPackagePayload>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
    where TPackagePayload : class, IPayload
{
    Task<IStatusResult<PackageStatus>> PublishPackageAsync(User user, TPackagePayload payload);

    Task<IStatusResult<PackageStatus>> UnpublishPackageAsync(User user, string name, string version);

    Task<IStatusResult<PackageStatus, TPackage[]>> GetPackagesAsync(User user, string name);

    Task<IStatusResult<PackageStatus>> ProcessDownloadAsync(User user, string name, string version, bool countDownload);
    Task<IReadOnlyCollection<string>> FindAllVersionsByNameAsync(string name);
    Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name);
    Task<TPackage?> TryFindByNameVersionAsync(string name, string version);
}