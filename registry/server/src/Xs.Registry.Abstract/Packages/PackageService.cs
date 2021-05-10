using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Execution;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Packages
{
    public class PackageService<TPackage, TPackageDependency, TPayload> : IPackageService<TPackage, TPackageDependency, TPayload> where TPayload : class, IPayload where TPackage : class, IPackage<TPackageDependency> where TPackageDependency : class, IPackageDependency
    {
        private readonly IMetaPackageRepository _metaPackageRepository;

        private readonly IMetaPackageManager _metaPackageManager;

        private readonly IPackageRepository<TPackage, TPackageDependency> _packageRepository;

        private readonly IPackageStorage _packageStorage;

        private readonly IPayloadParser<TPayload, TPackage, TPackageDependency> _payloadParser;

        private readonly ProjectType _projectType;

        public PackageService(
            IMetaPackageRepository metaPackageRepository,
            IMetaPackageManager metaPackageManager,
            IPackageRepository<TPackage, TPackageDependency> packageRepository,
            IPackageStorage packageStorage,
            IPayloadParser<TPayload, TPackage, TPackageDependency> payloadParser,
            ProjectType projectType
        )
        {
            _metaPackageRepository = metaPackageRepository;
            _metaPackageManager = metaPackageManager;
            _packageRepository = packageRepository;
            _packageStorage = packageStorage;
            _payloadParser = payloadParser;
            _projectType = projectType;
        }

        public async Task<IStatusResult<PackageStatus>> PublishPackageAsync(User user, TPayload payload)
        {
            var executor = Executor.Staged();

            var name = payload.Name;
            var version = payload.Version;

            // get metaPackage by (type, name)
            var metaPackage = await _metaPackageRepository.FindByTypeNameAsync(_projectType, name);

            var isNew = metaPackage == null;
            if (isNew)
                metaPackage = await _metaPackageRepository.CreateAsync(
                    _metaPackageManager.Generate(user, _projectType, payload)
                );

            var access = _metaPackageManager.GetAccess(metaPackage).ForUser(user);

            // if new - publish new package
            if (isNew)
                return await PublishNewPackageAsync(executor, metaPackage, access, payload);

            // check version presence
            var republished = await _packageRepository.FindByNameVersionAsync(name, version);

            // if present - republish package version, else - publish new package version
            return republished == null ?
                await PublishPackageVersionAsync(executor, metaPackage, access, payload) :
                await RepublishPackageVersionAsync(executor, metaPackage, access, payload);
        }

        public async Task<IStatusResult<PackageStatus>> UnpublishPackageAsync(User user, string name, string version)
        {
            // get available versions
            var versions = await _packageRepository.FindAllByNameAsync(name);
            if (!versions.Any(p => p.Version == version))
                return Result.Status(PackageStatus.NotFound);

            // load metaPackage and check permissions
            var metaPackage = await _metaPackageRepository.GetByIdAsync(versions[0].MetaPackageId);
            var access = _metaPackageManager.GetAccess(metaPackage).ForUser(user);
            if (!access.Has(Permission.Unpublish))
                return Result.Status(PackageStatus.Forbidden)
                    .Error("You need unpublish permission to unpublish this package.");

            var executor = Executor.Batch();

            // delete from storage
            executor.With(() => _packageStorage.DeleteAsync(name, version));

            // delete from db
            executor.With(() => _packageRepository.DeleteByNameVersionAsync(name, version));

            // if it was last package - delete metaPackage
            if (versions.Length == 1)
                executor.With(() => _metaPackageRepository.DeleteByIdAsync(metaPackage.Id));
            // else - update metaPackage
            else
            {
                // get latest version of all left except deleted (note - they are sorted from repository)
                var latest = versions.FirstOrDefault(p => p.Version != version);

                // if latest changed - need to update metaPackage
                if (latest.Version != metaPackage.Version)
                    executor.With(() => _metaPackageRepository.UpdateInfoAsync(metaPackage.Id, latest));

                // and anyway - recount downloads
                executor.With(
                    async() => await _metaPackageRepository.SetDownloadsAsync(
                        metaPackage.Id,
                        await _packageRepository.CountAllDownloadsAsync(metaPackage.Name)
                    )
                );
            }

            await executor.RunAsync();

            return Result.Status(PackageStatus.Ok);
        }

        public async Task<IStatusResult<PackageStatus, TPackage[]>> GetPackagesAsync(User user, string name)
        {
            var packages = await _packageRepository.FindAllByNameAsync(name);
            if (packages.Length == 0)
                return Result.Status(PackageStatus.NotFound, Array.Empty<TPackage>());

            var access = (await _metaPackageRepository.GetAccessByIdAsync(packages[0].MetaPackageId)).ForUser(user);
            if (!access.Has(Permission.Read))
                return Result.Status(PackageStatus.Forbidden, Array.Empty<TPackage>())
                    .Error("You need read permission to get this package.");

            return Result.Status(PackageStatus.Ok, packages);
        }

        public async Task<IStatusResult<PackageStatus>> ProcessDownloadAsync(User user, string name, string version, bool countDownload)
        {
            var package = await _packageRepository.FindByNameVersionAsync(name, version);
            if (package == null)
                return Result.Status(PackageStatus.NotFound);

            var access = (await _metaPackageRepository.GetAccessByIdAsync(package.MetaPackageId)).ForUser(user);
            if (!access.Has(Permission.Read))
                return Result.Status(PackageStatus.Forbidden)
                    .Error("You need read permission to get this package.");

            if (!(await _packageStorage.ExistsAsync(name, version)))
                return Result.Status(PackageStatus.InternalError)
                    .Error("Package file missing");

            if (countDownload)
            {
                await _packageRepository.IncrementDownloadsAsync(package.Id);
                var total = await _packageRepository.CountAllDownloadsAsync(package.Name);
                await _metaPackageRepository.SetDownloadsAsync(package.MetaPackageId, total);
            }

            return Result.Status(PackageStatus.Ok);
        }

        private async Task<IStatusResult<PackageStatus>> PublishNewPackageAsync(
            IStageExecutor executor,
            MetaPackage metaPackage,
            UserMetaPackageAccess access,
            TPayload payload
        )
        {
            // commit stage is missing, cause manually called earlier; so just deletion stage
            executor.Stage(
                () => { },
                () => _metaPackageRepository.DeleteByIdAsync(metaPackage.Id)
            );

            return await PublishPackageVersionAsync(executor, metaPackage, access, payload);
        }

        private async Task<IStatusResult<PackageStatus>> RepublishPackageVersionAsync(
            IStageExecutor executor,
            MetaPackage metaPackage,
            UserMetaPackageAccess access,
            TPayload payload
        )
        {
            if (!access.Has(Permission.Unpublish))
                return Result.Status(PackageStatus.Conflict)
                    .Error($"Package {payload.Name} {payload.Version} already exists. You need unpublish permission to overwrite it.");

            executor.Stage(
                async() =>
                {
                    await _packageStorage.DeleteAsync(payload.Name, payload.Version);
                    await _packageRepository.DeleteByNameVersionAsync(payload.Name, payload.Version);
                },
                () => { }
            );

            return await PublishPackageVersionAsync(executor, metaPackage, access, payload);
        }

        private async Task<IStatusResult<PackageStatus>> PublishPackageVersionAsync(
            IStageExecutor executor,
            MetaPackage metaPackage,
            UserMetaPackageAccess access,
            TPayload payload
        )
        {
            if (!access.Has(Permission.Publish))
                return Result.Status(PackageStatus.Forbidden)
                    .Error($"You need publish permission to publish package {payload.Name} {payload.Version}.");

            var pkg = _payloadParser.Parse(metaPackage.Id, payload);

            executor.Stage(
                () => _packageStorage.SaveAsync(pkg.Name, pkg.Version, payload.Stream),
                () => _packageStorage.DeleteAsync(pkg.Name, pkg.Version)
            );

            executor.Stage(
                () => _packageRepository.CreateAsync(pkg),
                () => _packageRepository.DeleteByNameVersionAsync(pkg.Name, pkg.Version)
            );

            executor.Stage(
                async() => await _metaPackageRepository.SetDownloadsAsync(
                    metaPackage.Id,
                    await _packageRepository.CountAllDownloadsAsync(pkg.Name)
                ),
                async() => await _metaPackageRepository.SetDownloadsAsync(
                    metaPackage.Id,
                    await _packageRepository.CountAllDownloadsAsync(pkg.Name)
                )
            );

            if (pkg.Version.CompareTo(metaPackage.Version) >= 0)
                executor.Stage(
                    () => _metaPackageRepository.UpdateInfoAsync(metaPackage.Id, payload),
                    () => _metaPackageRepository.UpdateInfoAsync(metaPackage.Id, metaPackage)
                );

            await executor.RunAsync();

            return Result.Status(PackageStatus.Ok);
        }
    }
}