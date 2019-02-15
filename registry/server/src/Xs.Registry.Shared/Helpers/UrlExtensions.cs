using System;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Shared.Helpers
{
    public static class UrlExtensions
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
}