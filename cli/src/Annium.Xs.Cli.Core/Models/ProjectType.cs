using Annium.Core.Mapper.Attributes;

namespace Annium.Xs.Cli.Core.Models;

[AutoMapped]
public enum ProjectType : byte
{
    None,
    Dotnet,
    Node,
}
