using System.IO;
using Annium.Core.Runtime.Types;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Dotnet.Tools;

[ResolutionKeyValue(ProjectType.Dotnet)]
internal sealed record SpecialConfiguration : Core.Tools.SpecialConfiguration
{
    public bool AddPreferredAttributes { get; init; } = true;
    public string DirectorySeparator { get; init; } = Path.DirectorySeparatorChar.ToString();
}
