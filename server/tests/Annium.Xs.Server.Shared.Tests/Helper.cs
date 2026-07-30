using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Internal.Repositories;
using Annium.Xs.Server.Shared.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Shared test-only builders used across multiple test classes in this project, so identical
/// arrange-phase logic isn't duplicated (and can't silently drift) between them.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Builds and persists a <see cref="MetaPackage"/>. Mirrors the real construction pattern used by
    /// <c>MetaPackageTool.Generate</c>: the permissions collection is a mutable <see cref="List{T}"/>
    /// passed into the <see cref="MetaPackage"/> constructor by reference, populated with
    /// <c>MetaPackageId = package.Id</c> only after construction (since <c>Id</c> is generated inside
    /// the constructor) — hand-building permissions with a placeholder id up front would silently
    /// persist rows pointing at the wrong package.
    /// </summary>
    public static async Task<MetaPackage> CreatePackageAsync(
        MetaPackageRepository repo,
        User owner,
        IReadOnlyCollection<MetaPackagePermission> permissions,
        ProjectType? type = null,
        string? name = null,
        int downloads = 0,
        string version = "1.0.0"
    )
    {
        var resolvedType = type ?? ProjectType.Register($"type-{Guid.NewGuid():N}");
        var permissionsList = new List<MetaPackagePermission>();
        var package = new MetaPackage(
            resolvedType,
            name ?? $"package-{Guid.NewGuid():N}",
            version,
            "description",
            Instant.FromUtc(2024, 1, 1, 0, 0),
            downloads,
            owner.Id,
            owner,
            permissionsList
        );
        foreach (var permission in permissions)
            permissionsList.Add(new MetaPackagePermission(package.Id, permission.Category, permission.Permission));

        await repo.CreateAsync(package);

        return package;
    }

    /// <summary>
    /// Builds a DI container with the given fakes registered the way <c>ServicePack</c> registers the
    /// real implementations (instance registration inferring the concrete type, then
    /// <c>AsInterfaces()</c>) — using an explicit interface type argument here instead would register
    /// zero descriptors, since <c>AsInterfaces()</c> reflects on the type argument itself.
    /// </summary>
    public static IServiceProviderContainer BuildProvider(
        FakeUserRepository userRepository,
        params FakeTokenAccessor[] accessors
    )
    {
        var container = new ServiceContainer();
        container.Add(userRepository).AsInterfaces().Singleton();
        foreach (var accessor in accessors)
            container.Add(accessor).AsInterfaces().Singleton();

        return container.BuildServiceProvider();
    }
}

/// <summary>
/// In-memory fake for the internal <see cref="IUserRepository"/>, used to drive
/// <c>AuthorizationFilter</c> tests without a real database. Only <see cref="TryFindByApiTokenAsync"/>
/// is exercised by the filter; every other member throws to surface an unexpected call instead of
/// silently returning a default.
/// </summary>
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _byToken = new();

    /// <summary>
    /// Number of times <see cref="TryFindByApiTokenAsync"/> has been called.
    /// </summary>
    public int TryFindByApiTokenAsyncCallCount { get; private set; }

    /// <summary>
    /// Registers a user, keyed by its <see cref="User.ApiToken"/>.
    /// </summary>
    /// <param name="user">The user to register.</param>
    public void Seed(User user) => _byToken[user.ApiToken] = user;

    /// <inheritdoc />
    public Task<User?> TryFindByApiTokenAsync(Guid token)
    {
        TryFindByApiTokenAsyncCallCount++;
        _byToken.TryGetValue(token, out var user);

        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task CreateAsync(User user) => throw new NotSupportedException("Not used by AuthorizationFilter tests.");

    /// <inheritdoc />
    public Task<User?> TryFindByLoginAsync(string login) =>
        throw new NotSupportedException("Not used by AuthorizationFilter tests.");

    /// <inheritdoc />
    public Task UpdateAsync(User user) => throw new NotSupportedException("Not used by AuthorizationFilter tests.");

    /// <inheritdoc />
    public Task UpdateApiTokenAsync(Guid userId, Guid apiToken) =>
        throw new NotSupportedException("Not used by AuthorizationFilter tests.");

    /// <inheritdoc />
    public Task DeleteByIdAsync(Guid id) => throw new NotSupportedException("Not used by AuthorizationFilter tests.");
}

/// <summary>
/// Configurable fake for <see cref="ITokenAccessor"/> that returns a fixed result and records how many
/// times it was invoked, so tests can assert <c>AuthorizationFilter</c>'s per-accessor loop precisely
/// (e.g. that a later accessor was, or was not, invoked at all).
/// </summary>
internal sealed class FakeTokenAccessor : ITokenAccessor
{
    private readonly Guid _token;
    private readonly IActionResult? _result;

    /// <summary>
    /// Number of times <see cref="GetToken"/> has been called.
    /// </summary>
    public int CallCount { get; private set; }

    /// <summary>
    /// Creates a fake that, on every call, returns <paramref name="token"/> paired with
    /// <paramref name="result"/> (leave <paramref name="result"/> <c>null</c> for a success case).
    /// </summary>
    /// <param name="token">The token to return.</param>
    /// <param name="result">The failure result to return, or <c>null</c> for success.</param>
    public FakeTokenAccessor(Guid token, IActionResult? result = null)
    {
        _token = token;
        _result = result;
    }

    /// <inheritdoc />
    public ValueTuple<Guid, IActionResult?> GetToken(HttpRequest request)
    {
        CallCount++;

        return (_token, _result);
    }
}
