using System;
using Annium.Core.Runtime.Types;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Node.Tools;

[ResolutionKeyValue(ProjectType.Node)]
internal sealed record SpecialConfiguration : Core.Tools.SpecialConfiguration
{
    public string[] PrivateScopes { get; private set; } = Array.Empty<string>();
}