using System;
using System.Net;
using Annium.Testing;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="BearerTokenAccessor"/>, pinning its header-presence check, segment-count and
/// scheme validation, guid parsing, and the whitespace-trimming applied to each split segment.
/// </summary>
public class BearerTokenAccessorTests
{
    private readonly BearerTokenAccessor _accessor = new();

    [Fact]
    public void GetToken_HeaderMissing_ReturnsUnauthorizedWithMessage()
    {
        // arrange
        var request = new DefaultHttpContext().Request;

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(Guid.Empty);
        var objectResult = (ObjectResult)result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Unauthorized);
        objectResult.Value.Is("Bearer authorization required.");
    }

    [Theory]
    [InlineData("Bearer")]
    [InlineData("a b c")]
    public void GetToken_WrongSegmentCount_ReturnsForbiddenWithFormatMessage(string header)
    {
        // arrange
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderNames.Authorization] = header;

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(Guid.Empty);
        var objectResult = (ObjectResult)result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Forbidden);
        objectResult.Value.Is("Authorization format is invalid.");
    }

    [Fact]
    public void GetToken_WrongScheme_ReturnsForbiddenWithBearerRequiredMessage()
    {
        // arrange
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderNames.Authorization] = $"Basic {Guid.NewGuid()}";

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(Guid.Empty);
        var objectResult = (ObjectResult)result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Forbidden);
        objectResult.Value.Is("Bearer authorization required.");
    }

    [Fact]
    public void GetToken_TokenSegmentNotAGuid_ReturnsForbiddenWithInvalidTokenMessage()
    {
        // arrange
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderNames.Authorization] = "Bearer not-a-guid";

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(Guid.Empty);
        var objectResult = (ObjectResult)result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Forbidden);
        objectResult.Value.Is("Invalid token passed");
    }

    [Fact]
    public void GetToken_ValidBearerToken_ReturnsTokenWithNullResult()
    {
        // arrange
        var expected = Guid.NewGuid();
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderNames.Authorization] = $"Bearer {expected}";

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(expected);
        result.IsNull();
    }

    [Fact]
    public void GetToken_SegmentsCarrySurroundingWhitespace_TrimsBeforeValidatingSchemeAndToken()
    {
        // arrange — a tab glued to the front of "Bearer" and around the token. Split(' ') only splits
        // on the single literal space between the two segments, so without the accessor's `.Trim()` the
        // scheme segment would be "\tBearer" (failing the `== "Bearer"` check and returning 403 "Bearer
        // authorization required." before the token is ever parsed). This pins that both segments are
        // trimmed first, so the scheme check and the guid parse both see the clean values.
        var expected = Guid.NewGuid();
        var request = new DefaultHttpContext().Request;
        request.Headers[HeaderNames.Authorization] = $"\tBearer \t{expected}\t";

        // act
        var (token, result) = _accessor.GetToken(request);

        // assert
        token.Is(expected);
        result.IsNull();
    }
}
