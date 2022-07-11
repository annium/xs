using System.IO;
using Annium.Core.Runtime.Types;

namespace Xs.Cli.Dotnet.Tools;

[ResolutionKeyValue(Constants.ProjectTypeString)]
internal sealed record SpecialConfiguration : Core.Tools.SpecialConfiguration
{
    public bool AddPreferredAttributes { get; init; } = true;
    public string DirectorySeparator { get; init; } = Path.DirectorySeparatorChar.ToString();
}