using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Execution;
using Annium.Linq;
using Server.Abstractions.Db.Repositories;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Shared.Domain.Enums;
using Server.Shared.Domain.Interfaces;
using Server.Shared.Domain.Models;
using Server.Shared.Repositories;
using Server.Shared.Tools;

namespace Server.Abstractions.Internal.Services;

internal class PackageService<TPackage, TPackageDependency, TPackageRequest>
    : IPackageService<TPackage, TPackageDependency, TPackageRequest>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
    where TPackageRequest : class, IPackageRequest
{
    private readonly IMetaPackageRepository _metaPackageRepository;
    private readonly IMetaPackageTool _metaPackageTool;
    private readonly IPackageRepository<TPackage, TPackageDependency> _packageRepository;
    private readonly IPackageStorage<TPackage, TPackageDependency> _packageStorage;
    private readonly IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest> _packageRequestParser;

    public PackageService(
        IMetaPackageRepository metaPackageRepository,
        IMetaPackageTool metaPackageTool,
        IPackageRepository<TPackage, TPackageDependency> packageRepository,
        IPackageStorage<TPackage, TPackageDependency> packageStorage,
        IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest> packageRequestParser
    )
    {
        _metaPackageRepository = metaPackageRepository;
        _metaPackageTool = metaPackageTool;
        _packageRepository = packageRepository;
        _packageStorage = packageStorage;
        _packageRequestParser = packageRequestParser;
    }

    public async Task<IStatusResult<PackageStatus, IReadOnlyCollection<TPackage>>> GetPackagesAsync(
        User user,
        string name
    )
    {
        var packages = await _packageRepository.FindAllByNameAsync(name);
        if (packages.Count == 0)
            return Result.Status(PackageStatus.NotFound, packages);

        var access = await _metaPackageRepository.TryGetAccessByIdAsync(packages.ElementAt(0).MetaPackageId);
        if (access is null || !access.ForUser(user).Has(Permission.Read))
            return Result
                .Status(PackageStatus.Forbidden, packages)
                .Error("You need read permission to get this package.");

        return Result.Status(PackageStatus.Ok, packages);
    }

    public async Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name)
    {
        return await _packageRepository.FindAllByNameAsync(name);
    }

    public async Task<TPackage?> TryFindByNameVersionAsync(string name, string version)
    {
        return await _packageRepository.TryFindByNameVersionAsync(name, version);
    }

    public async Task<IStatusResult<PackageStatus>> PublishPackageAsync(User user, TPackageRequest request)
    {
        var executor = Executor.Staged();

        var name = request.Name;
        var version = request.Version;

        // get metaPackage by (type, name)
        var metaPackage = await _metaPackageRepository.TryFindByTypeNameAsync(request.ProjectType, name);

        if (metaPackage is null)
        {
            metaPackage = _metaPackageTool.Generate(user, request.ProjectType, request);
            await _metaPackageRepository.CreateAsync(metaPackage);

            var access = _metaPackageTool.GetAccess(metaPackage).ForUser(user);

            return await PublishNewPackageAsync(executor, metaPackage, access, request);
        }
        else
        {
            // check version presence
            var republished = await _packageRepository.TryFindByNameVersionAsync(name, version);

            var access = _metaPackageTool.GetAccess(metaPackage).ForUser(user);

            // if present - republish package version, else - publish new package version
            return republished is null
                ? await PublishPackageVersionAsync(executor, metaPackage, access, request)
                : await RepublishPackageVersionAsync(executor, metaPackage, access, request);
        }
    }

    public async Task<IStatusResult<PackageStatus>> UnpublishPackageAsync(User user, string name, string version)
    {
        // get available versions
        var versions = await _packageRepository.FindAllByNameAsync(name);
        if (versions.None(x => x.Version == version))
            return Result.Status(PackageStatus.NotFound);

        // load metaPackage and check permissions
        var metaPackage = await _metaPackageRepository.TryGetByIdAsync(versions.ElementAt(0).MetaPackageId);
        if (metaPackage is null)
            return Result.Status(PackageStatus.NotFound);

        var access = _metaPackageTool.GetAccess(metaPackage).ForUser(user);
        if (!access.Has(Permission.Unpublish))
            return Result
                .Status(PackageStatus.Forbidden)
                .Error("You need unpublish permission to unpublish this package.");

        var executor = Executor.Batch();

        // delete from storage
        executor.With(() => _packageStorage.DeleteAsync(name, version));

        // delete from db
        executor.With(() => _packageRepository.DeleteByNameVersionAsync(name, version));

        // if it was last package - delete metaPackage
        if (versions.Count == 1)
            executor.With(() => _metaPackageRepository.DeleteByIdAsync(metaPackage.Id));
        // else - update metaPackage
        else
        {
            // get latest version of all left except deleted (note - they are sorted from repository)
            var latest = versions.First(p => p.Version != version);

            // if latest changed - need to update metaPackage
            if (latest.Version != metaPackage.Version)
                executor.With(() => _metaPackageRepository.UpdateInfoAsync(metaPackage.Id, latest));

            // and anyway - recount downloads
            executor.With(
                async () =>
                    await _metaPackageRepository.SetDownloadsAsync(
                        metaPackage.Id,
                        await _packageRepository.CountAllDownloadsAsync(metaPackage.Name)
                    )
            );
        }

        await executor.RunAsync();

        return Result.Status(PackageStatus.Ok);
    }

    public async Task<IStatusResult<PackageStatus>> ProcessDownloadAsync(
        User? user,
        string name,
        string version,
        bool countDownload
    )
    {
        var package = await _packageRepository.TryFindByNameVersionAsync(name, version);
        if (package is null)
            return Result.Status(PackageStatus.NotFound);

        var access = await _metaPackageRepository.TryGetAccessByIdAsync(package.MetaPackageId);
        if (access is null || !access.ForUser(user).Has(Permission.Read))
            return Result.Status(PackageStatus.Forbidden).Error("You need read permission to get this package.");

        if (!await _packageStorage.ExistsAsync(name, version))
            return Result.Status(PackageStatus.InternalError).Error("Package file missing");

        if (!countDownload)
            return Result.Status(PackageStatus.Ok);

        await _packageRepository.IncrementDownloadsAsync(package.Id);
        var total = await _packageRepository.CountAllDownloadsAsync(package.Name);
        await _metaPackageRepository.SetDownloadsAsync(package.MetaPackageId, total);

        return Result.Status(PackageStatus.Ok);
    }

    private async Task<IStatusResult<PackageStatus>> PublishNewPackageAsync(
        IStageExecutor executor,
        MetaPackage metaPackage,
        UserMetaPackageAccess access,
        TPackageRequest request
    )
    {
        // commit stage is missing, cause manually called earlier; so just deletion stage
        executor.Stage(() => { }, () => _metaPackageRepository.DeleteByIdAsync(metaPackage.Id));

        return await PublishPackageVersionAsync(executor, metaPackage, access, request);
    }

    private async Task<IStatusResult<PackageStatus>> RepublishPackageVersionAsync(
        IStageExecutor executor,
        MetaPackage metaPackage,
        UserMetaPackageAccess access,
        TPackageRequest request
    )
    {
        if (!access.Has(Permission.Unpublish))
            return Result
                .Status(PackageStatus.Conflict)
                .Error(
                    $"Package {request.Name} {request.Version} already exists. You need unpublish permission to overwrite it."
                );

        executor.Stage(
            async () =>
            {
                await _packageStorage.DeleteAsync(request.Name, request.Version);
                await _packageRepository.DeleteByNameVersionAsync(request.Name, request.Version);
            },
            () => { }
        );

        return await PublishPackageVersionAsync(executor, metaPackage, access, request);
    }

    private async Task<IStatusResult<PackageStatus>> PublishPackageVersionAsync(
        IStageExecutor executor,
        MetaPackage metaPackage,
        UserMetaPackageAccess access,
        TPackageRequest request
    )
    {
        if (!access.Has(Permission.Publish))
            return Result
                .Status(PackageStatus.Forbidden)
                .Error($"You need publish permission to publish package {request.Name} {request.Version}.");

        var pkg = _packageRequestParser.Parse(metaPackage, request);

        executor.Stage(
            () => _packageStorage.SaveAsync(pkg.Name, pkg.Version, request.Stream),
            () => _packageStorage.DeleteAsync(pkg.Name, pkg.Version)
        );

        executor.Stage(
            () => _packageRepository.CreateAsync(pkg),
            () => _packageRepository.DeleteByNameVersionAsync(pkg.Name, pkg.Version)
        );

        executor.Stage(
            async () =>
                await _metaPackageRepository.SetDownloadsAsync(
                    metaPackage.Id,
                    await _packageRepository.CountAllDownloadsAsync(pkg.Name)
                ),
            async () =>
                await _metaPackageRepository.SetDownloadsAsync(
                    metaPackage.Id,
                    await _packageRepository.CountAllDownloadsAsync(pkg.Name)
                )
        );

        if (string.Compare(pkg.Version, metaPackage.Version, StringComparison.Ordinal) >= 0)
            executor.Stage(
                () => _metaPackageRepository.UpdateInfoAsync(metaPackage.Id, request),
                () => _metaPackageRepository.UpdateInfoAsync(metaPackage.Id, metaPackage)
            );

        await executor.RunAsync();

        return Result.Status(PackageStatus.Ok);
    }
}
