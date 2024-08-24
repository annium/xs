using System.IO;
using Annium.Core.Runtime.Types;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Tools;

namespace Xx.Cli.Dotnet.Tools;

[ResolutionKeyValue(ProjectType.Dotnet)]
internal sealed record PlatformConfiguration : PlatformConfigurationBase
{
    public bool AddPreferredAttributes { get; init; } = true;
    public string DirectorySeparator { get; init; } = Path.DirectorySeparatorChar.ToString();
}
