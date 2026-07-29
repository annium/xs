using Annium.Testing;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Tests for the internal <see cref="FileStorageFactory"/>, pinning the memoization behaviour of
/// its <c>ConcurrentDictionary.GetOrAdd</c>-backed <c>Create</c> method.
/// </summary>
public class FileStorageFactoryTests : TestBase
{
    public FileStorageFactoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public void Create_SameRootCalledTwice_ReturnsSameInstance()
    {
        // arrange
        var factory = new FileStorageFactory();

        // act
        var first = factory.Create("root-a");
        var second = factory.Create("root-a");

        // assert
        ReferenceEquals(first, second).IsTrue();
    }

    [Fact]
    public void Create_DifferentRoots_ReturnsDistinctInstances()
    {
        // arrange
        var factory = new FileStorageFactory();

        // act
        var first = factory.Create("root-a");
        var second = factory.Create("root-b");

        // assert
        ReferenceEquals(first, second).IsFalse();
    }
}
