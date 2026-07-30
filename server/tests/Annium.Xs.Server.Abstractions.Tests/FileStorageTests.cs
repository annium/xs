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
        File.Exists(Path.Combine(root, "a", "b", "c", "file.txt")).IsFalse();
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
        File.Exists(Path.Combine(root, "a", "b", "file.txt")).IsFalse();
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
        File.Exists(Path.Combine(root, "a", "file.txt")).IsFalse();
        Directory.Exists(Path.Combine(root, "a")).IsFalse();
        Directory.Exists(root).IsTrue();
    }

    /// <summary>
    /// Deleting an absent artifact must be a no-op, including after the first delete removed the
    /// containing directory. Compensating logic (e.g. a publish rollback restoring an artifact)
    /// replays a delete before re-saving, and must not fault on the vanished directory.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_AlreadyDeletedAndDirectoryCleanedUp_IsNoOp()
    {
        // arrange
        var (storage, root) = CreateStorage();
        await storage.SaveAsync("a/file.txt", ToStream("content"));
        await storage.DeleteAsync("a/file.txt");
        Directory.Exists(Path.Combine(root, "a")).IsFalse();

        // act
        await storage.DeleteAsync("a/file.txt");

        // assert — and the path is still writable afterwards, which is what a restore needs
        await storage.SaveAsync("a/file.txt", ToStream("restored"));
        (await storage.ExistsAsync("a/file.txt")).IsTrue();
    }

    /// <summary>
    /// The artifact name is built from request-supplied package id/version, so a traversal segment
    /// must be rejected rather than resolving outside the storage root.
    /// </summary>
    [Theory]
    [InlineData("../escaped.bin")]
    [InlineData("../../etc/escaped.bin")]
    [InlineData("a/../../escaped.bin")]
    public async Task SaveAsync_NameEscapesRoot_Throws(string name)
    {
        // arrange
        var (storage, root) = CreateStorage();

        // act & assert
        await Wrap.It(async () => await storage.SaveAsync(name, ToStream("payload"))).ThrowsAsync<ArgumentException>();

        // nothing was written anywhere under the root. Asserting on the escaped path itself would
        // depend on state outside the root that this test cannot own or clean up.
        Directory.GetFileSystemEntries(root).IsEmpty();
    }

    [Theory]
    [InlineData("../escaped.bin")]
    [InlineData("../../etc/escaped.bin")]
    public async Task GetAsync_NameEscapesRoot_Throws(string name)
    {
        var (storage, _) = CreateStorage();

        await Wrap.It(async () => await storage.GetAsync(name)).ThrowsAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("../escaped.bin")]
    [InlineData("../../etc/escaped.bin")]
    public async Task DeleteAsync_NameEscapesRoot_Throws(string name)
    {
        var (storage, _) = CreateStorage();

        await Wrap.It(async () => await storage.DeleteAsync(name)).ThrowsAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_NeverSaved_IsNoOp()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        await storage.DeleteAsync("never/saved.txt");

        // assert
        (await storage.ExistsAsync("never/saved.txt")).IsFalse();
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

    [Fact]
    public async Task GetAsync_FileNotSaved_ThrowsFileNotFoundException()
    {
        // arrange — the parent directory exists (another file was saved into it) so the guard that
        // trips is specifically "file missing", not "directory missing"
        var (storage, _) = CreateStorage();
        await storage.SaveAsync("pkg/other.bin", ToStream("content"));

        // act & assert — GetAsync opens with FileMode.Open and performs no existence check of its own,
        // so a missing file surfaces as the underlying FileStream's own exception
        await Wrap.It(async () => await storage.GetAsync("pkg/missing.bin")).ThrowsAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task SaveAsync_NameAlreadySaved_ThrowsIOException()
    {
        // arrange
        var (storage, _) = CreateStorage();
        await storage.SaveAsync("pkg/1.0.0/file.bin", ToStream("first"));

        // act & assert — SaveAsync opens with FileMode.CreateNew, so re-saving the same name conflicts
        // with the file left behind by the first save
        await Wrap.It(async () => await storage.SaveAsync("pkg/1.0.0/file.bin", ToStream("second")))
            .ThrowsAsync<IOException>();
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
        exception.ParamName.Is("name");
        exception.Message.Is("Artifact name is empty. (Parameter 'name')");
    }

    [Fact]
    public void ExistsAsync_EmptyName_ThrowsArgumentException()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        var exception = Wrap.It(() => _ = storage.ExistsAsync(string.Empty)).Throws<ArgumentException>();

        // assert
        exception.ParamName.Is("name");
        exception.Message.Is("Artifact name is empty. (Parameter 'name')");
    }

    [Fact]
    public void ExistsAsync_WhitespaceName_ThrowsArgumentException()
    {
        // arrange
        var (storage, _) = CreateStorage();

        // act
        var exception = Wrap.It(() => _ = storage.ExistsAsync("   ")).Throws<ArgumentException>();

        // assert
        exception.ParamName.Is("name");
        exception.Message.Is("Artifact name is empty. (Parameter 'name')");
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
