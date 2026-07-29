using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Models;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Tests for the internal <c>PackageService</c>, pinning its dispatch, permission-guard, and
/// staged/batch execution behaviour (including the LIFO rollback semantics of <c>Executor.Staged()</c>
/// and the run-every-handler semantics of <c>Executor.Batch()</c>).
/// </summary>
public class PackageServiceTests : TestBase
{
    public PackageServiceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    #region PublishPackageAsync dispatch

    [Fact]
    public async Task PublishPackageAsync_MetaPackageMissing_GeneratesMetaPackageAndPublishesNewVersion()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.MetaPackageRepository.Created.Has(1);
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
        fixture.PackageRepository.Packages.Has(1);
        fixture.PackageRepository.Packages.At(0).Version.Is("1.0.0");
    }

    [Fact]
    public async Task PublishPackageAsync_MetaPackageExistsVersionMissing_PublishesNewVersion()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        var request = PackageServiceFixture.CreateRequest("pkg-a", "2.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.MetaPackageRepository.Created.IsEmpty();
        fixture.PackageStorage.Contains("pkg-a", "2.0.0").IsTrue();
        fixture.PackageRepository.Packages.Has(2);
        // "2.0.0" sorts above the meta-package's recorded "1.0.0" -> info stage runs too
        fixture.MetaPackageRepository.InfoUpdates.Has(1);
    }

    [Fact]
    public async Task PublishPackageAsync_MetaPackageExistsVersionPresent_Republishes()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish | Permission.Unpublish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", System.IO.Stream.Null);
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.PackageRepository.Packages.Has(1);
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
    }

    #endregion

    #region PublishPackageVersionAsync

    [Fact]
    public async Task PublishPackageAsync_MissingPublishPermission_ReturnsForbidden()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(user, "pkg-a", "1.0.0", Permission.Read);
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        var request = PackageServiceFixture.CreateRequest("pkg-a", "2.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.PlainError.Is("You need publish permission to publish package pkg-a 2.0.0.");
        fixture.PackageRepository.Packages.Has(1);
        fixture.PackageStorage.Contains("pkg-a", "2.0.0").IsFalse();
    }

    [Fact]
    public async Task PublishPackageAsync_AllStagesSucceed_RunsEveryStageAndReturnsOk()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        var request = PackageServiceFixture.CreateRequest("pkg-a", "2.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.PackageStorage.Contains("pkg-a", "2.0.0").IsTrue();
        fixture.PackageRepository.Packages.Any(p => p.Version == "2.0.0").IsTrue();
        fixture.MetaPackageRepository.DownloadsSet.Has(1);
        fixture.MetaPackageRepository.InfoUpdates.Has(1);
    }

    [Fact]
    public async Task PublishPackageAsync_RepositoryCreateThrows_RollsBackStorageSaveAndStillReturnsOk()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        fixture.PackageRepository.ThrowOnCreate = new InvalidOperationException("create boom");
        var request = PackageServiceFixture.CreateRequest("pkg-a", "2.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — SUSPECTED DEFECT: PublishPackageVersionAsync discards the staged executor's
        // result and unconditionally returns Ok, even though the pipeline actually failed.
        result.Status.Is(PackageStatus.Ok);

        // the storage save (stage 1) was committed, then rolled back once stage 2 (repo create) threw
        fixture.Log.Contains("Storage.Save:pkg-a:2.0.0").IsTrue();
        fixture.Log.Contains("Storage.Delete:pkg-a:2.0.0").IsTrue();
        fixture.PackageStorage.Contains("pkg-a", "2.0.0").IsFalse();

        // stage 2 never actually committed a package
        fixture.PackageRepository.Packages.Any(p => p.Version == "2.0.0").IsFalse();

        // downstream stages (downloads/info) never ran
        fixture.MetaPackageRepository.DownloadsSet.IsEmpty();
        fixture.MetaPackageRepository.InfoUpdates.IsEmpty();
    }

    [Fact]
    public async Task PublishPackageAsync_NewVersionDoesNotSortAboveCurrent_SkipsMetaPackageInfoUpdate()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "2.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "2.0.0"));
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
        // downloads recount always runs...
        fixture.MetaPackageRepository.DownloadsSet.Has(1);
        // ...but the version-info stage is conditional and "1.0.0" < "2.0.0" -> skipped
        fixture.MetaPackageRepository.InfoUpdates.IsEmpty();
    }

    #endregion

    #region RepublishPackageVersionAsync

    [Fact]
    public async Task PublishPackageAsync_RepublishMissingUnpublishPermission_ReturnsConflictWithMessage()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Conflict);
        result.PlainErrors.Has(1);
        result.PlainError.Is("Package pkg-a 1.0.0 already exists. You need unpublish permission to overwrite it.");
        fixture.PackageRepository.Packages.Has(1);
    }

    [Fact]
    public async Task PublishPackageAsync_RepublishWithPermission_DeletesOldVersionBeforePublishingNew()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish | Permission.Unpublish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", System.IO.Stream.Null);
        fixture.Log.Clear();
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert
        result.Status.Is(PackageStatus.Ok);
        var storageDeleteIndex = fixture.Log.IndexOf("Storage.Delete:pkg-a:1.0.0");
        var storageSaveIndex = fixture.Log.IndexOf("Storage.Save:pkg-a:1.0.0");
        var repoDeleteIndex = fixture.Log.IndexOf("Repo.Delete:pkg-a:1.0.0");
        var repoCreateIndex = fixture.Log.IndexOf("Repo.Create:pkg-a:1.0.0");

        (storageDeleteIndex >= 0).IsTrue();
        (storageSaveIndex >= 0).IsTrue();
        (repoDeleteIndex >= 0).IsTrue();
        (repoCreateIndex >= 0).IsTrue();
        (storageDeleteIndex < storageSaveIndex).IsTrue();
        (repoDeleteIndex < repoCreateIndex).IsTrue();
        fixture.PackageRepository.Packages.Has(1);
    }

    [Fact]
    public async Task PublishPackageAsync_RepublishRepositoryCreateThrows_LosesOldPackageDueToEmptyRollback()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish | Permission.Unpublish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", System.IO.Stream.Null);
        fixture.Log.Clear();
        fixture.PackageRepository.ThrowOnCreate = new InvalidOperationException("create boom");
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — SUSPECTED DEFECT (data loss): RepublishPackageVersionAsync registers its
        // delete-old-version stage with an empty rollback (`() => { }`). Its commit deletes the
        // pre-existing storage file AND db row as a single stage, so once that stage has
        // committed, the executor considers it "done" and there is nothing left to undo. When the
        // later repo-create stage throws, LIFO rollback only undoes the stages that come after the
        // delete stage (here, the storage save) — the delete-old-version stage's no-op rollback
        // leaves the old file/row deleted. The new version was never persisted either (create
        // threw), so the package "pkg-a" "1.0.0" is gone entirely, even though
        // PublishPackageVersionAsync unconditionally returns Ok.
        result.Status.Is(PackageStatus.Ok);

        // old row + old file are both gone and never restored
        fixture.PackageRepository.Packages.Any(p => p.Version == "1.0.0").IsFalse();
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsFalse();

        // the delete-old-version stage committed (storage delete + repo delete both logged)...
        fixture.Log.Contains("Storage.Delete:pkg-a:1.0.0").IsTrue();
        fixture.Log.Contains("Repo.Delete:pkg-a:1.0.0").IsTrue();
        // ...the new save was committed then rolled back...
        fixture.Log.Contains("Storage.Save:pkg-a:1.0.0").IsTrue();
        // ...and the new row was never created (ThrowOnCreate fired before logging)
        fixture.Log.Contains("Repo.Create:pkg-a:1.0.0").IsFalse();
    }

    #endregion

    #region UnpublishPackageAsync guards

    [Fact]
    public async Task UnpublishPackageAsync_VersionNotFound_ReturnsNotFound()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();

        // act
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "1.0.0");

        // assert
        result.Status.Is(PackageStatus.NotFound);
    }

    [Fact]
    public async Task UnpublishPackageAsync_MetaPackageNotFound_ReturnsNotFound()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var orphanMetaPackageId = Guid.NewGuid();
        fixture.PackageRepository.Seed(
            new TestPackage
            {
                MetaPackageId = orphanMetaPackageId,
                Name = "pkg-a",
                Version = "1.0.0",
            }
        );

        // act
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "1.0.0");

        // assert
        result.Status.Is(PackageStatus.NotFound);
    }

    [Fact]
    public async Task UnpublishPackageAsync_MissingUnpublishPermission_ReturnsForbiddenWithMessage()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));

        // act
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "1.0.0");

        // assert
        result.Status.Is(PackageStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.PlainError.Is("You need unpublish permission to unpublish this package.");
        fixture.PackageRepository.Packages.Has(1);
    }

    #endregion

    #region UnpublishPackageAsync batch branching

    [Fact]
    public async Task UnpublishPackageAsync_DeletingOnlyVersion_DeletesMetaPackage()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish | Permission.Unpublish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", System.IO.Stream.Null);

        // act
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "1.0.0");

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsFalse();
        fixture.PackageRepository.Packages.IsEmpty();
        fixture.MetaPackageRepository.Deleted.Has(1);
        fixture.MetaPackageRepository.Deleted.At(0).Is(metaPackage.Id);
        fixture.MetaPackageRepository.InfoUpdates.IsEmpty();
    }

    [Fact]
    public async Task UnpublishPackageAsync_DeletingLatestOfMultiple_UpdatesMetaPackageInfoAndDownloads()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        // meta-package's recorded version is the current latest ("2.0.0")
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "2.0.0",
            Permission.Read | Permission.Publish | Permission.Unpublish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0", downloads: 3));
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "2.0.0", downloads: 5));

        // act — delete the latest version; new latest becomes "1.0.0" != metaPackage.Version
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "2.0.0");

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.MetaPackageRepository.Deleted.IsEmpty();
        fixture.MetaPackageRepository.InfoUpdates.Has(1);
        fixture.MetaPackageRepository.InfoUpdates.At(0).Info.Version.Is("1.0.0");
        fixture.MetaPackageRepository.DownloadsSet.Has(1);
        fixture.MetaPackageRepository.DownloadsSet.At(0).Downloads.Is(3);
    }

    [Fact]
    public async Task UnpublishPackageAsync_DeletingNonLatestOfMultiple_SkipsInfoUpdateButRecomputesDownloads()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        // meta-package's recorded version already matches the surviving latest ("2.0.0")
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            user,
            "pkg-a",
            "2.0.0",
            Permission.Read | Permission.Publish | Permission.Unpublish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0", downloads: 3));
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "2.0.0", downloads: 5));

        // act — delete the non-latest version; surviving latest is still "2.0.0" == metaPackage.Version
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "1.0.0");

        // assert
        result.Status.Is(PackageStatus.Ok);
        fixture.MetaPackageRepository.InfoUpdates.IsEmpty();
        fixture.MetaPackageRepository.DownloadsSet.Has(1);
        fixture.MetaPackageRepository.DownloadsSet.At(0).Downloads.Is(5);
    }

    #endregion

    #region ProcessDownloadAsync

    [Fact]
    public async Task ProcessDownloadAsync_PackageNotFound_ReturnsNotFound()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();

        // act
        var result = await service.ProcessDownloadAsync(null, "pkg-a", "1.0.0", countDownload: false);

        // assert
        result.Status.Is(PackageStatus.NotFound);
    }

    [Fact]
    public async Task ProcessDownloadAsync_MetaPackageAccessMissing_ReturnsForbiddenWithMessage()
    {
        // arrange — orphan package: its MetaPackageId is never registered with the meta-package
        // repository, so TryGetAccessByIdAsync returns null and the "access is null" disjunct of
        // the guard (rather than the permission check) is what triggers the Forbidden result.
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var orphanMetaPackageId = Guid.NewGuid();
        fixture.PackageRepository.Seed(
            new TestPackage
            {
                MetaPackageId = orphanMetaPackageId,
                Name = "pkg-a",
                Version = "1.0.0",
            }
        );

        // act
        var result = await service.ProcessDownloadAsync(null, "pkg-a", "1.0.0", countDownload: true);

        // assert
        result.Status.Is(PackageStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.PlainError.Is("You need read permission to get this package.");
    }

    [Fact]
    public async Task ProcessDownloadAsync_MissingReadPermission_ReturnsForbidden()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var owner = PackageServiceFixture.CreateUser("owner");
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            owner,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish,
            worldPermission: Permission.None
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));

        // act — anonymous user gets world access, which grants nothing here
        var result = await service.ProcessDownloadAsync(null, "pkg-a", "1.0.0", countDownload: true);

        // assert
        result.Status.Is(PackageStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.PlainError.Is("You need read permission to get this package.");
    }

    [Fact]
    public async Task ProcessDownloadAsync_FileMissingInStorage_ReturnsInternalError()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var owner = PackageServiceFixture.CreateUser("owner");
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            owner,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish,
            worldPermission: Permission.Read
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));
        fixture.PackageStorage.FileExists = false;

        // act
        var result = await service.ProcessDownloadAsync(null, "pkg-a", "1.0.0", countDownload: true);

        // assert
        result.Status.Is(PackageStatus.InternalError);
        result.PlainErrors.Has(1);
        result.PlainError.Is("Package file missing");
    }

    [Fact]
    public async Task ProcessDownloadAsync_CountDownloadFalse_ReturnsOkWithoutCounting()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var owner = PackageServiceFixture.CreateUser("owner");
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            owner,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish,
            worldPermission: Permission.Read
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        var package = PackageServiceFixture.CreatePackage(metaPackage, "1.0.0", downloads: 4);
        fixture.PackageRepository.Seed(package);

        // act
        var result = await service.ProcessDownloadAsync(null, "pkg-a", "1.0.0", countDownload: false);

        // assert
        result.Status.Is(PackageStatus.Ok);
        package.Downloads.Is(4);
        fixture.MetaPackageRepository.DownloadsSet.IsEmpty();
    }

    [Fact]
    public async Task ProcessDownloadAsync_CountDownloadTrue_IncrementsAndPropagatesRecomputedTotal()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var owner = PackageServiceFixture.CreateUser("owner");
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            owner,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish,
            worldPermission: Permission.Read
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        var target = PackageServiceFixture.CreatePackage(metaPackage, "1.0.0", downloads: 4);
        fixture.PackageRepository.Seed(target);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "2.0.0", downloads: 1));

        // act
        var result = await service.ProcessDownloadAsync(null, "pkg-a", "1.0.0", countDownload: true);

        // assert
        result.Status.Is(PackageStatus.Ok);
        target.Downloads.Is(5);
        fixture.MetaPackageRepository.DownloadsSet.Has(1);
        fixture.MetaPackageRepository.DownloadsSet.At(0).Id.Is(metaPackage.Id);
        // recomputed total: (4 + 1 increment) + 1 from the other version
        fixture.MetaPackageRepository.DownloadsSet.At(0).Downloads.Is(6);
    }

    #endregion

    #region GetPackagesAsync

    [Fact]
    public async Task GetPackagesAsync_NoPackages_ReturnsNotFound()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();

        // act
        var result = await service.GetPackagesAsync(user, "pkg-a");

        // assert
        result.Status.Is(PackageStatus.NotFound);
        result.Data.IsEmpty();
    }

    [Fact]
    public async Task GetPackagesAsync_MetaPackageAccessMissing_ReturnsForbiddenWithMessage()
    {
        // arrange — orphan package: its MetaPackageId is never registered with the meta-package
        // repository, so TryGetAccessByIdAsync returns null and the "access is null" disjunct of
        // the guard (rather than the permission check) is what triggers the Forbidden result.
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        var orphanMetaPackageId = Guid.NewGuid();
        fixture.PackageRepository.Seed(
            new TestPackage
            {
                MetaPackageId = orphanMetaPackageId,
                Name = "pkg-a",
                Version = "1.0.0",
            }
        );

        // act
        var result = await service.GetPackagesAsync(user, "pkg-a");

        // assert
        result.Status.Is(PackageStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.PlainError.Is("You need read permission to get this package.");
    }

    [Fact]
    public async Task GetPackagesAsync_MissingReadPermission_ReturnsForbidden()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var owner = PackageServiceFixture.CreateUser("owner");
        var other = PackageServiceFixture.CreateUser("other");
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            owner,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish,
            worldPermission: Permission.None
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));

        // act
        var result = await service.GetPackagesAsync(other, "pkg-a");

        // assert
        result.Status.Is(PackageStatus.Forbidden);
        result.PlainErrors.Has(1);
        result.PlainError.Is("You need read permission to get this package.");
    }

    [Fact]
    public async Task GetPackagesAsync_HasReadPermission_ReturnsOk()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var owner = PackageServiceFixture.CreateUser("owner");
        var metaPackage = PackageServiceFixture.CreateMetaPackage(
            owner,
            "pkg-a",
            "1.0.0",
            Permission.Read | Permission.Publish
        );
        fixture.MetaPackageRepository.Seed(metaPackage);
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0"));

        // act
        var result = await service.GetPackagesAsync(owner, "pkg-a");

        // assert
        result.Status.Is(PackageStatus.Ok);
        result.Data.Has(1);
    }

    #endregion
}
