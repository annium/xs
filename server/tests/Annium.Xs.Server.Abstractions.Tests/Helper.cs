using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Abstractions.Internal.Db.Repositories;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Repositories;
using Annium.Xs.Server.Shared.Tools;
using NodaTime;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// A dependency of a package, used by <see cref="TestPackage"/>.
/// </summary>
internal sealed class TestPackageDependency : IPackageDependency
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
}

/// <summary>
/// A concrete, mutable stand-in for the generic <c>TPackage</c> parameter of <see cref="PackageService{TPackage,TPackageDependency,TPackageRequest}"/>.
/// </summary>
internal sealed class TestPackage : IPackage<TestPackageDependency>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MetaPackageId { get; set; }
    public int Downloads { get; set; }
    public IReadOnlyCollection<TestPackageDependency> Dependencies { get; set; } = Array.Empty<TestPackageDependency>();
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Instant Published { get; set; }
}

/// <summary>
/// A concrete, mutable stand-in for the generic <c>TPackageRequest</c> parameter.
/// </summary>
internal sealed class TestPackageRequest : IPackageRequest
{
    public required ProjectType ProjectType { get; init; }
    public Stream Stream { get; init; } = new MemoryStream();
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Instant Published { get; set; }
}

/// <summary>
/// In-memory fake for <see cref="IPackageRepository{TPackage,TPackageDependency}"/>. Mirrors the
/// ordering semantics of the real repository (<c>FindAllByNameAsync</c> is sorted by version,
/// descending, ordinal) so tests exercising "latest version" logic behave realistically.
/// </summary>
internal sealed class FakePackageRepository : IPackageRepository<TestPackage, TestPackageDependency>
{
    private readonly List<string> _log;
    private readonly List<TestPackage> _packages = new();

    public IReadOnlyCollection<TestPackage> Packages => _packages;

    /// <summary>
    /// When set, <see cref="CreateAsync"/> throws this exception instead of storing the package —
    /// used to simulate a mid-pipeline commit failure.
    /// </summary>
    public Exception? ThrowOnCreate { get; set; }

    public FakePackageRepository(List<string> log)
    {
        _log = log;
    }

    public void Seed(TestPackage package) => _packages.Add(package);

    public Task CreateAsync(TestPackage package)
    {
        if (ThrowOnCreate is not null)
            throw ThrowOnCreate;

        _packages.Add(package);
        _log.Add($"Repo.Create:{package.Name}:{package.Version}");

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<TestPackage>> FindAllByNameAsync(string name)
    {
        IReadOnlyCollection<TestPackage> result = _packages
            .Where(p => p.Name == name)
            .OrderByDescending(p => p.Version, StringComparer.Ordinal)
            .ToArray();

        return Task.FromResult(result);
    }

    public Task<TestPackage?> TryFindByNameVersionAsync(string name, string version)
    {
        var package = _packages.FirstOrDefault(p => p.Name == name && p.Version == version);

        return Task.FromResult(package);
    }

    public Task<int> CountAllDownloadsAsync(string name)
    {
        var total = _packages.Where(p => p.Name == name).Sum(p => p.Downloads);

        return Task.FromResult(total);
    }

    public Task IncrementDownloadsAsync(Guid id)
    {
        var package = _packages.FirstOrDefault(p => p.Id == id);
        if (package is not null)
            package.Downloads++;

        _log.Add($"Repo.Increment:{id}");

        return Task.CompletedTask;
    }

    public Task DeleteByNameVersionAsync(string name, string version)
    {
        _packages.RemoveAll(p => p.Name == name && p.Version == version);
        _log.Add($"Repo.Delete:{name}:{version}");

        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory fake for <see cref="IPackageStorage{TPackage,TPackageDependency}"/>.
/// </summary>
internal sealed class FakePackageStorage : IPackageStorage<TestPackage, TestPackageDependency>
{
    private readonly List<string> _log;
    private readonly HashSet<(string Name, string Version)> _files = new();

    /// <summary>
    /// The value returned by <see cref="ExistsAsync"/>. Defaults to <c>true</c>.
    /// </summary>
    public bool FileExists { get; set; } = true;

    public FakePackageStorage(List<string> log)
    {
        _log = log;
    }

    public bool Contains(string name, string version) => _files.Contains((name, version));

    public Task<bool> ExistsAsync(string name, string version) => Task.FromResult(FileExists);

    public Task SaveAsync(string name, string version, Stream stream)
    {
        _files.Add((name, version));
        _log.Add($"Storage.Save:{name}:{version}");

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string name, string version)
    {
        _files.Remove((name, version));
        _log.Add($"Storage.Delete:{name}:{version}");

        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory fake for <see cref="IMetaPackageRepository"/>.
/// </summary>
internal sealed class FakeMetaPackageRepository : IMetaPackageRepository
{
    private readonly List<string> _log;
    private readonly Dictionary<Guid, MetaPackage> _byId = new();

    public List<MetaPackage> Created { get; } = new();
    public List<Guid> Deleted { get; } = new();
    public List<(Guid Id, IPackageInfo Info)> InfoUpdates { get; } = new();
    public List<(Guid Id, int Downloads)> DownloadsSet { get; } = new();

    public FakeMetaPackageRepository(List<string> log)
    {
        _log = log;
    }

    public void Seed(MetaPackage metaPackage) => _byId[metaPackage.Id] = metaPackage;

    public Task CreateAsync(MetaPackage metaPackage)
    {
        _byId[metaPackage.Id] = metaPackage;
        Created.Add(metaPackage);
        _log.Add($"Meta.Create:{metaPackage.Id}");

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<MetaPackage>> FindAllAsync(
        Guid userId,
        ProjectType? type,
        string? query,
        int page,
        int count
    ) => throw new NotSupportedException("Not used by PackageService.");

    public Task<MetaPackage?> TryGetByIdAsync(Guid id)
    {
        _byId.TryGetValue(id, out var metaPackage);

        return Task.FromResult(metaPackage);
    }

    public Task<MetaPackageAccess?> TryGetAccessByIdAsync(Guid id)
    {
        if (!_byId.TryGetValue(id, out var metaPackage))
            return Task.FromResult<MetaPackageAccess?>(null);

        return Task.FromResult<MetaPackageAccess?>(new MetaPackageAccess(metaPackage.OwnerId, metaPackage.Permissions));
    }

    public Task<MetaPackage?> TryFindByTypeNameAsync(ProjectType type, string name)
    {
        var metaPackage = _byId.Values.FirstOrDefault(x => x.Type == type && x.Name == name);

        return Task.FromResult(metaPackage);
    }

    public Task UpdateInfoAsync(Guid id, IPackageInfo info)
    {
        InfoUpdates.Add((id, info));
        _log.Add($"Meta.UpdateInfo:{id}:{info.Name}:{info.Version}");

        return Task.CompletedTask;
    }

    public Task SetDownloadsAsync(Guid id, int downloads)
    {
        DownloadsSet.Add((id, downloads));
        _log.Add($"Meta.SetDownloads:{id}:{downloads}");

        return Task.CompletedTask;
    }

    public Task UpdatePermissionsAsync(Guid id, IReadOnlyCollection<MetaPackagePermission> permissions) =>
        throw new NotSupportedException("Not used by PackageService.");

    public Task DeleteByIdAsync(Guid id)
    {
        _byId.Remove(id);
        Deleted.Add(id);
        _log.Add($"Meta.Delete:{id}");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Fake for <see cref="IMetaPackageTool"/>. <see cref="Generate"/> mirrors the production tool's
/// permission grants (owner gets Read|Publish, world gets None) since that grant is what makes the
/// "brand-new meta-package" publish path succeed.
/// </summary>
internal sealed class FakeMetaPackageTool : IMetaPackageTool
{
    public MetaPackage Generate(User user, ProjectType type, IPackageInfo package)
    {
        var permissions = new List<MetaPackagePermission>();
        var metaPackage = new MetaPackage(
            type,
            package.Name,
            package.Version,
            package.Description,
            package.Published,
            0,
            user.Id,
            user,
            permissions
        );

        permissions.Add(
            new MetaPackagePermission(metaPackage.Id, PermissionCategory.Owner, Permission.Read | Permission.Publish)
        );
        permissions.Add(new MetaPackagePermission(metaPackage.Id, PermissionCategory.World, Permission.None));

        return metaPackage;
    }

    public MetaPackageAccess GetAccess(MetaPackage metaPackage) => new(metaPackage.OwnerId, metaPackage.Permissions);
}

/// <summary>
/// Fake for <see cref="IPackageRequestParser{TPackage,TPackageDependency,TPackageRequest}"/>.
/// </summary>
internal sealed class FakePackageRequestParser
    : IPackageRequestParser<TestPackage, TestPackageDependency, TestPackageRequest>
{
    public TestPackage Parse(MetaPackage metaPackage, TestPackageRequest request) =>
        new()
        {
            Id = Guid.NewGuid(),
            MetaPackageId = metaPackage.Id,
            Downloads = 0,
            Dependencies = Array.Empty<TestPackageDependency>(),
            Name = request.Name,
            Version = request.Version,
            Description = request.Description,
            Published = request.Published,
        };
}

/// <summary>
/// Bundles every fake dependency of <see cref="PackageService{TPackage,TPackageDependency,TPackageRequest}"/>
/// behind a single shared operation log, so tests can assert cross-dependency call ordering (e.g.
/// rollback undoing a storage save, or republish deleting before re-creating).
/// </summary>
internal sealed class PackageServiceFixture
{
    /// <summary>
    /// The type registered for all packages created via this fixture. <see cref="ProjectType"/> is a
    /// process-wide registry keyed by name, so tests share the same instance under this name.
    /// </summary>
    public static readonly ProjectType ProjectType = ProjectType.Register("xs-test-project-type");

    public List<string> Log { get; } = new();
    public FakeMetaPackageRepository MetaPackageRepository { get; }
    public FakeMetaPackageTool MetaPackageTool { get; } = new();
    public FakePackageRepository PackageRepository { get; }
    public FakePackageStorage PackageStorage { get; }
    public FakePackageRequestParser PackageRequestParser { get; } = new();

    public PackageServiceFixture()
    {
        MetaPackageRepository = new FakeMetaPackageRepository(Log);
        PackageRepository = new FakePackageRepository(Log);
        PackageStorage = new FakePackageStorage(Log);
    }

    public IPackageService<TestPackage, TestPackageDependency, TestPackageRequest> CreateService() =>
        new PackageService<TestPackage, TestPackageDependency, TestPackageRequest>(
            MetaPackageRepository,
            MetaPackageTool,
            PackageRepository,
            PackageStorage,
            PackageRequestParser
        );

    public static User CreateUser(string login = "user") => new(login, "hash", Guid.NewGuid());

    /// <summary>
    /// Builds a <see cref="MetaPackage"/> with explicit owner/world permission grants, mirroring how
    /// <see cref="FakeMetaPackageTool.Generate"/> wires permissions to the generated meta-package id.
    /// </summary>
    public static MetaPackage CreateMetaPackage(
        User owner,
        string name,
        string version,
        Permission ownerPermission,
        Permission worldPermission = Permission.None,
        int downloads = 0
    )
    {
        var permissions = new List<MetaPackagePermission>();
        var metaPackage = new MetaPackage(
            ProjectType,
            name,
            version,
            "description",
            Instant.FromUtc(2024, 1, 1, 0, 0),
            downloads,
            owner.Id,
            owner,
            permissions
        );

        permissions.Add(new MetaPackagePermission(metaPackage.Id, PermissionCategory.Owner, ownerPermission));
        permissions.Add(new MetaPackagePermission(metaPackage.Id, PermissionCategory.World, worldPermission));

        return metaPackage;
    }

    public static TestPackage CreatePackage(MetaPackage metaPackage, string version, int downloads = 0) =>
        new()
        {
            Id = Guid.NewGuid(),
            MetaPackageId = metaPackage.Id,
            Downloads = downloads,
            Name = metaPackage.Name,
            Version = version,
            Description = "description",
            Published = Instant.FromUtc(2024, 1, 1, 0, 0),
        };

    public static TestPackageRequest CreateRequest(string name, string version) =>
        new()
        {
            ProjectType = ProjectType,
            Name = name,
            Version = version,
            Description = "description",
            Published = Instant.FromUtc(2024, 1, 1, 0, 0),
        };
}
