using System.IO;
using Annium.Core.Runtime.Types;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Tools;

namespace Annium.Xs.Cli.Dotnet.Tools;

[ResolutionKeyValue(ProjectType.Dotnet)]
internal sealed record PlatformConfiguration : PlatformConfigurationBase
{
    public bool AddPreferredAttributes { get; init; } = true;
    public string DirectorySeparator { get; init; } = Path.DirectorySeparatorChar.ToString();
}
