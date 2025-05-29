using System;
using Annium.Xs.Server.Abstractions.Tools;

namespace Annium.Xs.Server.Abstractions.Internal.Tools;

internal class UrlTool : IUrlTool
{
    private readonly Uri _baseUrl;

    public UrlTool(Uri baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public Uri AbsoluteUrl(string relativePath) => new(_baseUrl, relativePath);
}
