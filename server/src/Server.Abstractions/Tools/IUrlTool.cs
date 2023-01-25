using System;

namespace Server.Abstractions.Tools;

public interface IUrlTool
{
    Uri AbsoluteUrl(string relativePath);
}