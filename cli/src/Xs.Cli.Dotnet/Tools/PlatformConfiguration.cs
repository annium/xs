using System.IO;
using Annium.Core.Runtime.Types;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Dotnet.Tools;

[ResolutionKeyValue(ProjectType.Dotnet)]
internal sealed record PlatformConfiguration : PlatformConfigurationBase
{
    public bool AddPreferredAttributes { get; init; } = true;
    public string DirectorySeparator { get; init; } = Path.DirectorySeparatorChar.ToString();
}
