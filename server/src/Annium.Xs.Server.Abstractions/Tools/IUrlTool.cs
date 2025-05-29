using System;

namespace Annium.Xs.Server.Abstractions.Tools;

public interface IUrlTool
{
    Uri AbsoluteUrl(string relativePath);
}
