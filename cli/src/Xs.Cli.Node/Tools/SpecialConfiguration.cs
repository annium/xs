using System;
using Annium.Core.Runtime.Types;

namespace Xs.Cli.Node.Tools;

[ResolutionKeyValue(Constants.ProjectTypeString)]
internal sealed record SpecialConfiguration : Core.Tools.SpecialConfiguration
{
    public string[] PrivateScopes { get; private set; } = Array.Empty<string>();
}