using Annium.Core.Runtime.Types;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Tools;

namespace Annium.Xs.Cli.Node.Tools;

[ResolutionKeyValue(ProjectType.Node)]
internal sealed record PlatformConfiguration : PlatformConfigurationBase
{
    public string[] PrivateScopes { get; private set; } = [];
}
