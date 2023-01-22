using System;
using Microsoft.AspNetCore.Mvc;

namespace Server.Shared.Extensions;

public static class UrlHelperExtensions
{
    public static Uri AbsoluteUri(this IUrlHelper url, string relativePath)
    {
        var request = url.ActionContext.HttpContext.Request;

        return new Uri(
            new Uri($"{request.Scheme}://{request.Host.ToUriComponent()}{request.PathBase.ToUriComponent()}"),
            relativePath
        );
    }
}