using System;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Models;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="ProjectType"/>'s process-wide static registry: <see cref="ProjectType.Register"/>
/// and <see cref="ProjectType.Get"/>. Every name used here is unique to a given test run (suffixed with
/// a fresh guid), since the registry (<c>ProjectType._types</c>) is a single static list shared by every
/// test class in the process — reusing a literal name risks colliding with other test classes running
/// in parallel.
/// </summary>
public class ProjectTypeTests
{
    [Fact]
    public void Get_NameNeverRegistered_ThrowsWithNameInMessage()
    {
        // arrange
        var name = UniqueName();

        // act
        var exception = Wrap.It(() => ProjectType.Get(name)).Throws<Exception>();

        // assert
        exception.Message.Is($"Project type {name} is not registered.");
    }

    [Fact]
    public void Register_ThenGet_ReturnsSameInstance()
    {
        // arrange
        var name = UniqueName();

        // act
        var registered = ProjectType.Register(name);
        var fetched = ProjectType.Get(name);

        // assert
        ReferenceEquals(registered, fetched).IsTrue();
    }

    [Fact]
    public void Register_CalledTwiceWithSameName_IsIdempotentAndReturnsSameInstance()
    {
        // arrange
        var name = UniqueName();

        // act
        var first = ProjectType.Register(name);
        var second = ProjectType.Register(name);

        // assert
        ReferenceEquals(first, second).IsTrue();
    }

    [Fact]
    public void ToString_ReturnsRegisteredName()
    {
        // arrange
        var name = UniqueName();

        // act
        var projectType = ProjectType.Register(name);

        // assert
        projectType.ToString().Is(name);
    }

    private static string UniqueName() => $"xs-shared-tests-project-type-{Guid.NewGuid():N}";
}
