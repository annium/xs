using System;
using System.Net;
using Annium.Testing;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="HeaderTokenAccessor"/>, pinning its configured-header presence check
/// (including the header name interpolated into the missing-header message) and guid parsing.
/// </summary>
public class HeaderTokenAccessorTests
{
    private const string HeaderName = "X-Test-Token";

    private readonly HeaderTokenAccessor _accessor = new(HeaderName);

    [Fact]
    public void GetToken_HeaderMissing_ReturnsUnauthorizedWithHeaderNameInterpolated()
    {
        // arrange
        var request = new DefaultHttpContext().Request;

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(Guid.Empty);
        var objectResult = (ObjectResult)result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Unauthorized);
        objectResult.Value.Is($"Authorization with '{HeaderName}' header required.");
    }

    [Fact]
    public void GetToken_HeaderPresentButNotAGuid_ReturnsForbiddenWithInvalidTokenMessage()
    {
        // arrange
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderName] = "not-a-guid";

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(Guid.Empty);
        var objectResult = (ObjectResult)result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Forbidden);
        objectResult.Value.Is("Invalid token passed");
    }

    [Fact]
    public void GetToken_HeaderPresentAndValidGuid_ReturnsTokenWithNullResult()
    {
        // arrange
        var expected = Guid.NewGuid();
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderName] = expected.ToString();

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(expected);
        result.IsNull();
    }
}
