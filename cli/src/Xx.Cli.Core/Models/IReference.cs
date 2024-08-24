namespace Xx.Cli.Core.Models;

public interface IReference
{
    ProjectType Type { get; }
    string Name { get; }
    Version Version { get; }
}
