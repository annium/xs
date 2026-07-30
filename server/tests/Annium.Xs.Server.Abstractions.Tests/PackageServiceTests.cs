using System;
using System.IO;
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

    [Fact]
    public async Task PublishPackageAsync_MetaPackageMissingAndRepositoryCreateThrows_RollsBackOrphanMetaPackage()
    {
        // arrange
        var fixture = new PackageServiceFixture();
        var service = fixture.CreateService();
        var user = PackageServiceFixture.CreateUser();
        fixture.PackageRepository.ThrowOnCreate = new InvalidOperationException("create boom");
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — the new-package path's rollback-only stage (commit no-op, rollback deletes the
        // just-generated meta-package) is only reachable when a later stage fails; this pins that it
        // actually fires instead of leaving an orphan meta-package behind. The pipeline itself still
        // failed (repo create threw), so the status is InternalError (see FIX 1).
        result.Status.Is(PackageStatus.InternalError);
        fixture.MetaPackageRepository.Created.Has(1);
        var metaPackageId = fixture.MetaPackageRepository.Created.At(0).Id;
        fixture.MetaPackageRepository.Deleted.Has(1);
        fixture.MetaPackageRepository.Deleted.At(0).Is(metaPackageId);
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsFalse();
        fixture.PackageRepository.Packages.IsEmpty();
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
    public async Task PublishPackageAsync_RepositoryCreateThrows_RollsBackStorageSaveAndReturnsInternalError()
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

        // assert — the staged executor's result reports failure, so the pipeline must surface it
        // as InternalError instead of unconditionally returning Ok.
        result.Status.Is(PackageStatus.InternalError);
        result.PlainErrors.Has(1);
        result.PlainError.Is("create boom");

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

    [Fact]
    public async Task PublishPackageAsync_NewVersionEqualsCurrentMetaVersion_RunsMetaPackageInfoUpdate()
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
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "0.5.0"));
        // request version equals the meta-package's recorded version, and is not itself already
        // published, so this exercises the "==" edge of the ">=" guard directly (not the ">" case
        // covered elsewhere, nor the republish overload, which never reaches this comparison)
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — pins that ">=" (not just ">") registers the meta-package info-update stage
        result.Status.Is(PackageStatus.Ok);
        fixture.MetaPackageRepository.InfoUpdates.Has(1);
        fixture.MetaPackageRepository.InfoUpdates.At(0).Id.Is(metaPackage.Id);
    }

    [Fact]
    public async Task PublishPackageAsync_MetaPackageInfoUpdateThrows_RollsBackRepoCreateAndStorageSaveWithMatchingRecount()
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
        fixture.PackageRepository.Seed(PackageServiceFixture.CreatePackage(metaPackage, "1.0.0", downloads: 3));
        fixture.MetaPackageRepository.ThrowOnUpdateInfo = new InvalidOperationException("update info boom");
        // "2.0.0" sorts above the meta-package's recorded "1.0.0" -> the info-update stage (the only
        // stage registered after recount-downloads) is actually registered
        var request = PackageServiceFixture.CreateRequest("pkg-a", "2.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — the info-update stage's commit throws, so LIFO rollback undoes the recount-downloads,
        // repo-create and storage-save stages ahead of it (the info-update stage itself never committed,
        // so it is not rolled back).
        result.Status.Is(PackageStatus.InternalError);
        result.PlainErrors.Has(1);
        result.PlainError.Is("update info boom");

        fixture.PackageRepository.Packages.Any(p => p.Version == "2.0.0").IsFalse();
        fixture.PackageStorage.Contains("pkg-a", "2.0.0").IsFalse();

        // recount-downloads' commit and rollback are literally the same delegate. LIFO rollback order
        // runs it before the repo-create rollback removes "2.0.0", so both calls recompute the exact
        // same total from the exact same repository state — that is what makes sharing one delegate
        // for both directions correct.
        var setDownloadsLog = fixture.Log.Where(l => l.StartsWith("Meta.SetDownloads:")).ToArray();
        setDownloadsLog.Has(2);
        setDownloadsLog.At(0).Is(setDownloadsLog.At(1));
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
    public async Task PublishPackageAsync_RepublishRepositoryCreateThrows_RestoresOldPackageOnRollback()
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
        var originalBytes = new byte[] { 11, 22, 33, 44, 55 };
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", new MemoryStream(originalBytes));
        fixture.Log.Clear();
        fixture.PackageRepository.ThrowOnCreate = new InvalidOperationException("create boom");
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — RepublishPackageVersionAsync splits the delete-old-version work into two
        // reversible stages (storage delete with a buffered-content restore rollback, db delete
        // with a captured-row restore rollback), so when the later repo-create stage throws, LIFO
        // rollback restores both the old storage file and the old db row instead of leaving the
        // package gone entirely. The pipeline still failed (repo create threw), so the status is
        // InternalError.
        result.Status.Is(PackageStatus.InternalError);

        // old row + old file are both restored by rollback, and the restored file content is the
        // original bytes byte-for-byte (not just present, and not e.g. an empty/wrong buffer)
        fixture.PackageRepository.Packages.Any(p => p.Version == "1.0.0").IsTrue();
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
        fixture.PackageStorage.GetBytes("pkg-a", "1.0.0").SequenceEqual(originalBytes).IsTrue();

        // the delete-old-version stages committed (storage delete + repo delete both logged)...
        fixture.Log.Contains("Storage.Delete:pkg-a:1.0.0").IsTrue();
        fixture.Log.Contains("Repo.Delete:pkg-a:1.0.0").IsTrue();
        // ...the new save was committed then rolled back...
        fixture.Log.Contains("Storage.Save:pkg-a:1.0.0").IsTrue();
        // ...and the sole "Repo.Create:pkg-a:1.0.0" log entry comes from the rollback restoring the
        // old row — ThrowOnCreate fired once, on the new-row attempt, before it could log anything
        fixture.Log.Count(l => l == "Repo.Create:pkg-a:1.0.0").Is(1);
    }

    [Fact]
    public async Task PublishPackageAsync_RepublishNewArtifactSaveThrowsAfterPartialWrite_RestoresOldArtifactContentWithoutOrphanCollision()
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
        var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", new MemoryStream(originalBytes));
        fixture.Log.Clear();
        // simulates an ecosystem-specific save (e.g. Dotnet's PackageStorage deriving a nuspec from a
        // malformed upload, or a transient I/O error mid-CopyToAsync) that persists the new artifact's
        // bytes before failing — the exact failure PublishPackageVersionAsync's storage-save stage must
        // clean up after itself, and that RepublishPackageVersionAsync's stage A rollback must be able
        // to restore over regardless.
        fixture.PackageStorage.ThrowOnSave = new IOException("partial write boom");
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — the pipeline still failed (the new artifact's save threw), so InternalError...
        result.Status.Is(PackageStatus.InternalError);
        // ...but the old artifact must be restored, byte-for-byte, and not lost to a swallowed
        // rollback IOException caused by colliding with the orphaned partially-written new artifact
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
        fixture.PackageStorage.GetBytes("pkg-a", "1.0.0").SequenceEqual(originalBytes).IsTrue();
        fixture.PackageRepository.Packages.Any(p => p.Version == "1.0.0").IsTrue();
    }

    [Fact]
    public async Task PublishPackageAsync_RepublishOldArtifactGetThrows_ReturnsInternalErrorLeavingRowAndArtifactUntouched()
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
        await fixture.PackageStorage.SaveAsync("pkg-a", "1.0.0", new MemoryStream(new byte[] { 9, 8, 7 }));
        fixture.Log.Clear();
        fixture.PackageStorage.ThrowOnGet = new InvalidOperationException("get boom");
        var request = PackageServiceFixture.CreateRequest("pkg-a", "1.0.0");

        // act
        var result = await service.PublishPackageAsync(user, request);

        // assert — stage A's commit reads the old artifact into a buffer before deleting it; when the
        // read itself throws, the delete is never reached, so nothing committed and there's nothing
        // for a rollback to undo. Both the pre-existing db row and storage artifact stay untouched.
        result.Status.Is(PackageStatus.InternalError);
        result.PlainErrors.Has(1);
        result.PlainError.Is("get boom");
        fixture.PackageRepository.Packages.Any(p => p.Version == "1.0.0").IsTrue();
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
        fixture.Log.IsEmpty();
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

    [Fact]
    public async Task UnpublishPackageAsync_StorageDeleteThrows_StillRunsRemainingBatchStagesAndReturnsInternalError()
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
        fixture.PackageStorage.ThrowOnDelete = new InvalidOperationException("delete boom");

        // act
        var result = await service.UnpublishPackageAsync(user, "pkg-a", "1.0.0");

        // assert — Executor.Batch() catches each handler's exception independently and keeps going,
        // unlike the LIFO short-circuit of Executor.Staged(): the storage-delete stage throwing does
        // not stop the repo-delete or meta-package-delete stages (registered after it) from running.
        // The batch result still reports the storage-delete failure, so UnpublishPackageAsync must
        // surface it as InternalError instead of unconditionally returning Ok.
        result.Status.Is(PackageStatus.InternalError);
        result.PlainErrors.Has(1);
        result.PlainError.Is("delete boom");
        fixture.Log.Contains("Repo.Delete:pkg-a:1.0.0").IsTrue();
        fixture.Log.Contains($"Meta.Delete:{metaPackage.Id}").IsTrue();
        fixture.MetaPackageRepository.Deleted.Has(1);
        fixture.MetaPackageRepository.Deleted.At(0).Is(metaPackage.Id);
        fixture.PackageRepository.Packages.IsEmpty();
        // the storage delete itself threw, so the file was never actually removed
        fixture.PackageStorage.Contains("pkg-a", "1.0.0").IsTrue();
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
