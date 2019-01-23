using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal interface ISpecialProject : IProject
    {
        Version Version { get; set; }
    }
}