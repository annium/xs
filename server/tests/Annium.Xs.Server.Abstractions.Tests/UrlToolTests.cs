using System;
using Annium.Testing;
using Annium.Xs.Server.Abstractions.Internal.Tools;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Tests for the internal <see cref="UrlTool"/>, pinning the <c>new Uri(baseUri, relativePath)</c>
/// resolution semantics it delegates to — including the classic trailing-slash gotcha where a
/// base URL without a trailing slash drops its last path segment during resolution.
/// </summary>
public class UrlToolTests : TestBase
{
    public UrlToolTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public void AbsoluteUrl_BaseWithTrailingSlashRelativeWithoutLeadingSlash_AppendsUnderBasePath()
    {
        // arrange
        var tool = new UrlTool(new Uri("http://example.com/api/"));

        // act
        var url = tool.AbsoluteUrl("v1/packages");

        // assert — trailing slash on base is preserved as a directory, so the relative path is
        // appended underneath it
        url.ToString().Is("http://example.com/api/v1/packages");
    }

    [Fact]
    public void AbsoluteUrl_BaseWithoutTrailingSlashRelativeWithoutLeadingSlash_DropsLastBaseSegment()
    {
        // arrange
        var tool = new UrlTool(new Uri("http://example.com/api"));

        // act
        var url = tool.AbsoluteUrl("v1/packages");

        // assert — SUSPECTED DEFECT-ADJACENT GOTCHA: without a trailing slash, "api" is treated as
        // a file name (not a directory) by Uri's relative-resolution algorithm, so it is discarded
        // and the relative path is merged in its place instead of underneath it.
        url.ToString().Is("http://example.com/v1/packages");
    }

    [Fact]
    public void AbsoluteUrl_RelativeWithLeadingSlashBaseHasTrailingSlash_ReplacesEntirePath()
    {
        // arrange
        var tool = new UrlTool(new Uri("http://example.com/api/"));

        // act
        var url = tool.AbsoluteUrl("/v1/packages");

        // assert — a relative path starting with "/" is an absolute-path reference: it replaces
        // the base path entirely, regardless of the base's trailing slash
        url.ToString().Is("http://example.com/v1/packages");
    }

    [Fact]
    public void AbsoluteUrl_RelativeWithLeadingSlashBaseHasNoTrailingSlash_ReplacesEntirePath()
    {
        // arrange
        var tool = new UrlTool(new Uri("http://example.com/api"));

        // act
        var url = tool.AbsoluteUrl("/v1/packages");

        // assert — same absolute-path-reference behaviour regardless of base's trailing slash
        url.ToString().Is("http://example.com/v1/packages");
    }
}
