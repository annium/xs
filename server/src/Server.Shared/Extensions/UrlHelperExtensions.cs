using System;
using Microsoft.AspNetCore.Mvc;

namespace Server.Shared.Extensions;

public static class UrlHelperExtensions
{
    public static Uri AbsoluteUri(this IUrlHelper url, string relativePath)
    {
        var request = url.ActionContext.HttpContext.Request;
        var baseUri = new Uri($"{request.Scheme}://{request.Host.ToUriComponent()}{request.PathBase.ToUriComponent()}");

        return new Uri(baseUri, relativePath);
    }
}