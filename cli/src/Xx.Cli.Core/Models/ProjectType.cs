using Annium.Core.Mapper.Attributes;

namespace Xx.Cli.Core.Models;

[AutoMapped]
public enum ProjectType : byte
{
    None,
    Dotnet,
    Node
}
