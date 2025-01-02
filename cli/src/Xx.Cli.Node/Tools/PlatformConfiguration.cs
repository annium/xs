using System;
using Annium.Core.Runtime.Types;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Tools;

namespace Xx.Cli.Node.Tools;

[ResolutionKeyValue(ProjectType.Node)]
internal sealed record PlatformConfiguration : PlatformConfigurationBase
{
    public string[] PrivateScopes { get; private set; } = [];
}
