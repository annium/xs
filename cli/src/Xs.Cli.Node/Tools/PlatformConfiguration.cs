using System;
using Annium.Core.Runtime.Types;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools;

[ResolutionKeyValue(ProjectType.Node)]
internal sealed record PlatformConfiguration : PlatformConfigurationBase
{
    public string[] PrivateScopes { get; private set; } = Array.Empty<string>();
}
