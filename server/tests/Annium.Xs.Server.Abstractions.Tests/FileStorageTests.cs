using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Tests for the internal <see cref="FileStorage"/>, pinning its path-validation guard, save/read
/// round-trip, and the recursive empty-directory cleanup performed by <c>DeleteAsync</c>.
/// </summary>
public class FileStorageTests : TestBase
{
    /// <summary>
    /// Temp directories created by <see cref="CreateStorage"/>, removed in <see cref="DisposeAsync"/>.
    /// </summary>
    private readonly List<string> _roots = new();

    public FileStorageTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    #region DeleteAsync recursive cleanup

    [Fact]
    public async Task DeleteAsync_NestedEmptyDirectories_RemovesAncestorsUpToButNotIncludingRoot()
    {
        // arrange
        var (storage, root) = CreateStorage();
        await storage.SaveAsync("a/b/c/file.txt", ToStream("content"));

        // act
        await storage.DeleteAsync("a/b/c/file.txt");

        // assert
        Directory.Exists(root).IsTrue();
        Directory.Exists(Path.Combine(root, "a")).IsFalse();
        Directory.Exists(Path.Combine(root, "a", "b")).IsFalse();
        Directory.Exists(Path.Combine(root, "a", "b", "c")).IsFalse();
    }

    [Fact]
    public async Task DeleteAsync_AncestorStillContainsOtherFile_StopsCleanupAtThatAncestor()
    {
        // arrange
        var (storage, root) = CreateStorage();
        await storage.SaveAsync("a/b/file.txt", ToStream("target"));
        await storage.SaveAsync("a/other.txt", ToStream("keep-me"));

        // act
        await storage.DeleteAsync("a/b/file.txt");

        // assert
        // "a/b" became empty after the delete -> removed
        Directory.Exists(Path.Combine(root, "a", "b")).IsFalse();
        // "a" still contains "other.txt" -> preserved
        Directory.Exists(Path.Combine(root, "a")).IsTrue();
        File.Exists(Path.Combine(root, "a", "other.txt")).IsTrue();
    }

    [Fact]
    public async Task DeleteAsync_SingleLevelEmptyDirectory_RemovesDirectory()
    {
        // arrange
        var (storage, root) = CreateStorage();
        await storage.SaveAsync("a/file.txt", ToStream("content"));

        // act
        await storage.DeleteAsync("a/file.txt");

        // assert
        Directory.Exists(Path.Combine(root, "a")).IsFalse();
        Directory.Exists(root).IsTrue();
    }

    #endregion

    #region SaveAsync / ExistsAsync / GetAsync round-trip

    [Fact]
    public async Task SaveAsync_ThenExistsAndGet_RoundTripsContentFaithfully()
    {
        // arrange
        var (storage, _) = CreateStorage();
        const string content = "hello, file storage";

        // act
        await storage.SaveAsync("pkg/1.0.0/file.bin", ToStream(content));
        var exists = await storage.ExistsAsync("pkg/1.0.0/file.bin");
        await using var readStream = await storage.GetAsync("pkg/1.0.0/file.bin");
        using var reader = new StreamReader(readStream, Encoding.UTF8);
        var read = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        // assert
        exists.IsTrue();
        read.Is(content);
    }

    [Fact]
    public async Task ExistsAsync_FileNotSaved_ReturnsFalse()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        var exists = await storage.ExistsAsync("missing/file.bin");

        // assert
        exists.IsFalse();
    }

    #endregion

    #region GetPath validation guard

    [Fact]
    public void ExistsAsync_NullName_ThrowsArgumentException()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        var exception = Wrap.It(() => _ = storage.ExistsAsync(null!)).Throws<ArgumentException>();

        // assert
        exception.Message.Is("Given  is empty.");
    }

    [Fact]
    public void ExistsAsync_EmptyName_ThrowsArgumentException()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        var exception = Wrap.It(() => _ = storage.ExistsAsync(string.Empty)).Throws<ArgumentException>();

        // assert
        exception.Message.Is("Given  is empty.");
    }

    [Fact]
    public void ExistsAsync_WhitespaceName_ThrowsArgumentException()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        var exception = Wrap.It(() => _ = storage.ExistsAsync("   ")).Throws<ArgumentException>();

        // assert
        exception.Message.Is("Given     is empty.");
    }

    #endregion

    public override async ValueTask DisposeAsync()
    {
        foreach (var root in _roots)
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        await base.DisposeAsync();
    }

    private (FileStorage Storage, string Root) CreateStorage()
    {
        var root = Path.Combine(Path.GetTempPath(), "xs-file-storage-tests", Guid.NewGuid().ToString("N"));
        _roots.Add(root);

        return (new FileStorage(root), Path.GetFullPath(root));
    }

    private static MemoryStream ToStream(string content) => new(Encoding.UTF8.GetBytes(content));
}
