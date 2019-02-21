using System.Linq;
using System.Threading.Tasks;
using Xs.Execution;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Packages
{
    public class PackageService<TPackage, TPackageDependency, TPayload> : IPackageService<TPackage, TPackageDependency, TPayload> where TPayload : class, IPayload where TPackage : class, IPackage<TPackageDependency> where TPackageDependency : class, IPackageDependency
    {
        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IMetaPackageManager metaPackageManager;

        private readonly IPackageRepository<TPackage, TPackageDependency> packageRepository;

        private readonly IPackageStorage packageStorage;

        private readonly IPayloadParser<TPayload, TPackage, TPackageDependency> payloadParser;

        private readonly ProjectType projectType;

        public PackageService(
            IMetaPackageRepository metaPackageRepository,
            IMetaPackageManager metaPackageManager,
            IPackageRepository<TPackage, TPackageDependency> packageRepository,
            IPackageStorage packageStorage,
            IPayloadParser<TPayload, TPackage, TPackageDependency> payloadParser,
            ProjectType projectType
        )
        {
            this.metaPackageRepository = metaPackageRepository;
            this.metaPackageManager = metaPackageManager;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
            this.payloadParser = payloadParser;
            this.projectType = projectType;
        }

        public async Task<IPackageResult> PublishPackageAsync(User user, TPayload payload)
        {
            var executor = Executor.Staged();

            var name = payload.Name;
            var version = payload.Version;

            // get metaPackage by (type, name)
            var metaPackage = await metaPackageRepository.FindByTypeNameAsync(projectType, name);

            var isNew = metaPackage == null;
            if (isNew)
                metaPackage = await metaPackageRepository.CreateAsync(
                    metaPackageManager.Generate(user, projectType, payload)
                );

            var access = metaPackageManager.GetAccess(metaPackage).ForUser(user);

            // if new - publish new package
            if (isNew)
                return await PublishNewPackageAsync(executor, metaPackage, access, payload);

            // check version presence
            var republished = await packageRepository.FindByNameVersionAsync(name, version);

            // if present - republish package version, else - publish new package version
            return republished == null ?
                await PublishPackageVersionAsync(executor, metaPackage, access, payload) :
                await RepublishPackageVersionAsync(executor, metaPackage, access, payload);
        }

        public async Task<IPackageResult> UnpublishPackageAsync(User user, string name, string version)
        {
            // get available versions
            var versions = await packageRepository.FindAllByNameAsync(name);
            if (!versions.Any(p => p.Version == version))
                return new NotFoundResult();

            // load metaPackage and check permissions
            var metaPackage = await metaPackageRepository.GetByIdAsync(versions[0].MetaPackageId);
            var access = metaPackageManager.GetAccess(metaPackage).ForUser(user);
            if (!access.Has(Permission.Unpublish))
                return new ForbiddenResult("You need unpublish permission to unpublish this package.");

            var executor = Executor.Batch();

            // delete from storage
            executor.With(() => packageStorage.DeleteAsync(name, version));

            // delete from db
            executor.With(() => packageRepository.DeleteByNameVersionAsync(name, version));

            // if it was last package - delete metaPackage
            if (versions.Length == 1)
                executor.With(() => metaPackageRepository.DeleteByIdAsync(metaPackage.Id));
            // else - update metaPackage
            else
            {
                // get latest version of all left except deleted (note - they are sorted from repository)
                var latest = versions.FirstOrDefault(p => p.Version != version);

                // if latest changed - need to update metaPackage
                if (latest.Version != metaPackage.Version)
                    executor.With(() => metaPackageRepository.UpdateInfoAsync(metaPackage.Id, latest));

                // and anyway - recount downloads
                executor.With(
                    async() => await metaPackageRepository.SetDownloadsAsync(
                        metaPackage.Id,
                        await packageRepository.CountAllDownloadsAsync(metaPackage.Name)
                    )
                );
            }

            await executor.RunAsync();

            return new NoContentResult();
        }

        public async Task<IPackageResult> GetPackagesAsync(User user, string name)
        {
            var packages = await packageRepository.FindAllByNameAsync(name);
            if (packages.Length == 0)
                return new NotFoundResult();

            var access = (await metaPackageRepository.GetAccessByIdAsync(packages[0].MetaPackageId)).ForUser(user);
            if (!access.Has(Permission.Read))
                return new ForbiddenResult("You need read permission to get this package.");

            return new ArrayResult<TPackage>(packages);
        }

        public async Task<IPackageResult> TrackDownloadAsync(User user, string name, string version)
        {
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            if (package == null)
                return new NotFoundResult();

            if (!(await packageStorage.ExistsAsync(name, version)))
                return new InternalErrorResult("Package file missing");

            await packageRepository.IncrementDownloadsAsync(package.Id);
            var total = await packageRepository.CountAllDownloadsAsync(package.Name);
            await metaPackageRepository.SetDownloadsAsync(package.MetaPackageId, total);

            return null;
        }

        private async Task<IPackageResult> PublishNewPackageAsync(
            StageExecutor executor,
            MetaPackage metaPackage,
            UserMetaPackageAccess access,
            TPayload payload
        )
        {
            // commit stage is missing, cause manually called earlier; so just deletion stage
            executor.Stage(
                () => { },
                () => metaPackageRepository.DeleteByIdAsync(metaPackage.Id)
            );

            return await PublishPackageVersionAsync(executor, metaPackage, access, payload);
        }

        private async Task<IPackageResult> RepublishPackageVersionAsync(
            StageExecutor executor,
            MetaPackage metaPackage,
            UserMetaPackageAccess access,
            TPayload payload
        )
        {
            if (!access.Has(Permission.Unpublish))
                return new ConflictResult($"Package {payload.Name} {payload.Version} already exists. You need unpublish permission to overwrite it.");

            executor.Stage(
                async() =>
                {
                    await packageStorage.DeleteAsync(payload.Name, payload.Version);
                    await packageRepository.DeleteByNameVersionAsync(payload.Name, payload.Version);
                },
                () => { }
            );

            return await PublishPackageVersionAsync(executor, metaPackage, access, payload);
        }

        private async Task<IPackageResult> PublishPackageVersionAsync(
            StageExecutor executor,
            MetaPackage metaPackage,
            UserMetaPackageAccess access,
            TPayload payload
        )
        {
            if (!access.Has(Permission.Publish))
                return new ForbiddenResult($"You need publish permission to publish package {payload.Name} {payload.Version}.");

            var pkg = payloadParser.Parse(metaPackage.Id, payload);

            executor.Stage(
                () => packageStorage.SaveAsync(pkg.Name, pkg.Version, payload.Stream),
                () => packageStorage.DeleteAsync(pkg.Name, pkg.Version)
            );

            executor.Stage(
                () => packageRepository.CreateAsync(pkg),
                () => packageRepository.DeleteByNameVersionAsync(pkg.Name, pkg.Version)
            );

            executor.Stage(
                async() => await metaPackageRepository.SetDownloadsAsync(
                    metaPackage.Id,
                    await packageRepository.CountAllDownloadsAsync(pkg.Name)
                ),
                async() => await metaPackageRepository.SetDownloadsAsync(
                    metaPackage.Id,
                    await packageRepository.CountAllDownloadsAsync(pkg.Name)
                )
            );

            if (pkg.Version.CompareTo(metaPackage.Version) > 0)
                executor.Stage(
                    () => metaPackageRepository.UpdateInfoAsync(metaPackage.Id, payload),
                    () => metaPackageRepository.UpdateInfoAsync(metaPackage.Id, metaPackage)
                );

            await executor.RunAsync();

            return new NoContentResult();
        }
    }
}